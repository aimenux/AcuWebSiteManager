<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM301000.aspx.cs" Inherits="Page_PM301000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ProjectEntry" PrimaryView="Project" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Bill" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreateChangeOrder" CommitChanges="true" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Report" />
            <px:PXDSCallbackCommand Name="ProjectBalanceReport" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateTemplate" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AutoBudget" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="LockCommitments" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UnlockCommitments" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="LockBudget" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UnlockBudget" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RunAllocation" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ValidateBalance" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ChangeID" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CopyProject" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Hold" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Forecast" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="UpdateRetainage" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewReleaseRetainage" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Name="SetCurrencyRates" Visible="False" />
            <px:PXDSCallbackCommand Name="CurrencyRates" Visible="False" />
                       
            <px:PXDSCallbackCommand DependOnGrid="TaskGrid" Name="ViewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand DependOnGrid="CostBudgetGrid" Name="ViewCostCommitments" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="CostBudgetGrid" Name="ViewCostTransactions" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="CostBudgetGrid" Name="ViewCostBudgetInventory" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="RevenueBudgetGrid" Name="ViewRevenueTransactions" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="RevenueBudgetGrid" Name="ViewRevenueCommitments" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="RevenueBudgetGrid" Name="ViewRevenueBudgetInventory" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="ProjectBalanceGrid" Name="ViewBalanceTransactions" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="ProjectBalanceGrid" Name="ViewCommitments" Visible="False" />
            <px:PXDSCallbackCommand Name="AddAllVendorClasses" Visible="False" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewOrigDocument" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewProforma" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Name="ViewOrigChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Name="ViewPurchaseOrder" Visible="False" DependOnGrid="PurchaseOrdersGrid" />
			<px:PXDSCallbackCommand Name="CreatePurchaseOrder" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateSubcontract" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewChangeRequest" Visible="false" />
            <px:PXDSCallbackCommand Name="AddTasks" Visible="False" CommitChanges="True" PostData="Page" />
            <px:PXDSCallbackCommand Name="ActivateTasks" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CompleteTasks" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
            <px:PXDSCallbackCommand Name="ComplianceDocument$PurchaseOrder$Link" Visible="false" DependOnGrid="grid" CommitChanges="True" />
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
    <style type="text/css">
	[id$=_RevenueBudgetGrid], [id$=_CostBudgetGrid] {
        border-top:solid 1px #D2D4D7 !important;
	}
    [id$=_RevenueFilter_content], [id$=_CostFilter_content] { border-top:none}
        </style>    
    
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Project" Caption="Project Summary" FilesIndicator="True"
        NoteIndicator="True" NotifyIndicator="true" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" DataSourceID="ds" AutoRefresh="True">
                <GridProperties FastFilterFields="Description, CustomerID, CustomerID_Customer_acctName" />
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" AllowAddNew="True" AllowEdit="True" AutoRefresh="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" DataSourceID="ds" AllowAddNew="True" AllowEdit="True" AutoRefresh="True"/>
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
			<pxa:PXCurrencyRate ID="edCury" DataField="CuryIDCopy" runat="server" DataSourceID="ds" RateTypeView="_PMProject_CurrencyInfo_" DataMember="_Currency_" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
                <AutoCallBack Command="Hold" Target="ds">
                </AutoCallBack>
            </px:PXCheckBox>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXNumberEdit ID="edIncome" runat="server" DataField="TaskTotals.CuryIncome" Enabled="False" />
            <px:PXNumberEdit ID="edExpense" runat="server" DataField="TaskTotals.CuryExpense" Enabled="False" />
			<px:PXNumberEdit ID="edMargin" runat="server" DataField="TaskTotals.CuryMargin" Enabled="False" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XXS" ControlSize="XXS" />
			<px:PXLabel runat="server" />
			<px:PXLabel runat="server" />
			<px:PXNumberEdit ID="edMarginPct" runat="server" DataField="TaskTotals.MarginPct" Enabled="False" />
        </Template>
    </px:PXFormView>
    
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="ProjectProperties">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Summary">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Project Properties" />
                    <px:PXDropDown ID="edBudgetLevel" runat="server" DataField="BudgetLevel" CommitChanges="True" />
                    <px:PXDropDown ID="edCostBudgetLevel" runat="server" DataField="CostBudgetLevel" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edStartDate0" runat="server" DataField="StartDate" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
                    <px:PXSelector ID="edApprover" runat="server" DataField="ApproverID" />
                    <px:PXTextEdit ID="edSiteAddress" runat="server" DataField="SiteAddress" />
                    <px:PXTextEdit ID="edLastChangeOrderNumber" runat="server" DataField="LastChangeOrderNumber" />
                    <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="S" CommitChanges="True" />
					<px:PXButton ID="btnSetCurrencyRates" runat="server" CommandName="SetCurrencyRates" CommandSourceID="ds" Size="xs" Text="Set Rates" Height="20" />
                    <px:PXLayoutRule ID="PXLayoutRule13" runat="server" />
                    <px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" Size="S" CommitChanges="True" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
                    <px:PXCheckBox ID="edChangeOrderWorkflow" runat="server" DataField="ChangeOrderWorkflow" CommitChanges="True"/>
                    <px:PXCheckBox runat="server" DataField="AllowNonProjectAccountGroups" ID="edAllowNonProjectAccountGroups" />
                    <px:PXCheckBox ID="chkRestrictToEmployeeList" runat="server" DataField="RestrictToEmployeeList" CommitChanges="True"/>
                    <px:PXCheckBox ID="chkRestrictToResourceList" runat="server" DataField="RestrictToResourceList" CommitChanges="True"/>
                    <px:PXCheckBox ID="edBudgetMetrics" runat="server" DataField="BudgetMetricsEnabled" CommitChanges="True"/>
                    <px:PXCheckBox ID="edCertifiedJob" runat="server" DataField="CertifiedJob"/>
                    <px:PXLayoutRule runat="server" GroupCaption="Billing And Allocation Settings" StartGroup="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="edBillingCuryID" runat="server" DataField="BillingCuryID" Size="S" CommitChanges="True" />
                    <px:PXFormView ID="billingForm" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" Size="S" />
                            <px:PXDateTimeEdit ID="edNextDate" runat="server" DataField="NextDate" />
                            <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSelector ID="edLocation" runat="server" DataField="LocationID" />
                    <px:PXSelector ID="edPayrollWorkLocationID" runat="server" DataField="PayrollWorkLocationID" />
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" />
                    <px:PXSelector ID="edAllocationID" runat="server" DataField="AllocationID" AllowEdit="True" AllowAddNew="True" CommitChanges="true" />
                    <px:PXCheckBox ID="chkAutoAllocate" runat="server" DataField="AutoAllocate" />
                    <px:PXSelector ID="edBillingID" runat="server" DataField="BillingID" AllowEdit="True" AllowAddNew="True" CommitChanges="true" />
                    <px:PXSelector ID="edDefaultBranchID" runat="server" DataField="DefaultBranchID" />
                    <px:PXSelector ID="edRateTable" runat="server" DataField="RateTableID" AllowEdit="True" AllowAddNew="True" CommitChanges="true" />

                    <px:PXCheckBox ID="chkCreateProforma" runat="server" DataField="CreateProforma" CommitChanges="True" />
                    <px:PXCheckBox ID="edLimitsEnabled" runat="server" DataField="LimitsEnabled" CommitChanges="true" />
                    <px:PXCheckBox ID="edPrepaymentEnabled" runat="server" DataField="PrepaymentEnabled" CommitChanges="true" />
                    <px:PXSelector ID="edPrepaymentDefCode" runat="server" DataField="PrepaymentDefCode" />
                    <px:PXCheckBox ID="chkAutomaticReleaseAR" runat="server" DataField="AutomaticReleaseAR" />
                   
                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Bill-to" StartGroup="True" LabelsWidth="SM" ControlSize="XM" />
                    
                    <px:PXLayoutRule runat="server" GroupCaption="BILL-TO CONTACT" StartGroup="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXFormView ID="Billing_Contact" runat="server" DataMember="Billing_Contact" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
                            <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommandSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="Billing_Address" runat="server" Caption="BILL-TO ADDRESS" DataMember="Billing_Address" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" Height="18px" />
                            <px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" />
                            <px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
                        </Template>
                    </px:PXFormView>

                    <px:PXLayoutRule runat="server" ID="RetainageRule1" StartGroup="True" GroupCaption="RETAINAGE" />
                    <px:PXDropDown runat="server" ID="edRetainageMode" DataField="RetainageMode" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule2" Merge="True" />
                    <px:PXNumberEdit runat="server" DataField="ProjectRevenueTotals.CuryAmount" ID="edContractAmount1" />
                    <px:PXCheckBox runat="server" ID="edIncludeCO" DataField="IncludeCO" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule3" Merge="False" />
                    <px:PXNumberEdit runat="server" DataField="ProjectRevenueTotals.CuryRevisedAmount" ID="edContractAmount2" />
                    <px:PXNumberEdit runat="server" DataField="ProjectRevenueTotals.ContractCompletedPct" ID="edContractCompletedPct1" />
                    <px:PXNumberEdit runat="server" DataField="ProjectRevenueTotals.ContractCompletedWithCOPct" ID="edContractCompletedPct2" />
                    <px:PXNumberEdit runat="server" DataField="ProjectRevenueTotals.CuryTotalRetainedAmount" ID="edTotalRetainedAmount" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule4" Merge="True" />
                    <px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" CommitChanges="true" ></px:PXNumberEdit>
                    <px:PXCheckBox runat="server" ID="edSteppedRetainage" DataField="SteppedRetainage" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule5" Merge="False" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule6" Merge="True" />
                    <px:PXNumberEdit runat="server" ID="edRetainageMaxPct" DataField="RetainageMaxPct" CommitChanges="True" />
                    <px:PXNumberEdit runat="server" ID="edCuryCapAmount" DataField="CuryCapAmount" SuppressLabel="True" />
                    <px:PXLayoutRule runat="server" ID="RetainageRule7" Merge="False" />
                    <px:PXGrid runat="server" ID="gridRetainageSteps" SyncPosition="True" FilesIndicator="False" AllowFilter="False" AllowSearch="False" Width="400" Height="120" Caption="Stepped Retainage" CaptionVisible="True" AllowPaging="False" AdjustPageSize="None" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="RetainageSteps">
                                <RowTemplate>
                                    <px:PXTextEdit runat="server" ID="edStepThresholdPct" DataField="ThresholdPct" />
                                    <px:PXTextEdit runat="server" ID="edStepRetainagePct" DataField="RetainagePct" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ThresholdPct" Width="180" />
                                    <px:PXGridColumn DataField="RetainagePct" Width="180" /></Columns></px:PXGridLevel></Levels>
                         <AutoSize Enabled="False" />
                          <Mode AllowAddNew="True" AllowDelete="True" AllowSort="False" /></px:PXGrid>

                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Visibility Settings" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInGL" runat="server" DataField="VisibleInGL" />
                    <px:PXCheckBox ID="chkVisibleInAP" runat="server" DataField="VisibleInAP" />
                    <px:PXCheckBox ID="chkVisibleInAR" runat="server" DataField="VisibleInAR" />
                    <px:PXCheckBox ID="chkVisibleInSO" runat="server" DataField="VisibleInSO" />
                    <px:PXCheckBox ID="chkVisibleInPO" runat="server" DataField="VisibleInPO" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInIN" runat="server" DataField="VisibleInIN" />
                    <px:PXCheckBox ID="chkVisibleInCA" runat="server" DataField="VisibleInCA" />
                    <px:PXCheckBox ID="chkVisibleInCR" runat="server" DataField="VisibleInCR" />
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInTA" runat="server" DataField="VisibleInTA" />
                    <px:PXCheckBox ID="chkVisibleInEA" runat="server" DataField="VisibleInEA" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />

                     <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartGroup="True" GroupCaption="Quote" />
                    <px:PXSelector ID="edQuoteNbr" runat="server" DataField="QuoteNbr" Enabled="false" AllowEdit="True"/>
                           
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tasks">
                <Template>
                    <px:PXGrid runat="server" ID="TaskGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Tasks" DataKeyNames="ProjectID,TaskCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edProjectTaskCD2" runat="server" DataField="TaskCD" />
                                    <px:PXSegmentMask Size="s" ID="edDefaultSubID2" runat="server" DataField="DefaultSubID" />
                                    <px:PXSelector ID="edRateTableID2" runat="server" DataField="RateTableID" />
                                    <px:PXSelector Size="s" ID="edAllocationID2" runat="server" DataField="AllocationID" AllowAddNew="True" AllowEdit="True" />
									<px:PXSelector Size="s" ID="edBillingID2" runat="server" DataField="BillingID" AllowAddNew="True" AllowEdit="True" />
                                    <px:PXDropDown Size="m" ID="edBillingOption2" runat="server" DataField="BillingOption" />
                                    <px:PXSegmentMask ID="edDefaultAccountID2" runat="server" DataField="DefaultAccountID" />
                                    <px:PXTextEdit ID="edDescription2" runat="server" DataField="Description" />
                                    <px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID" />
                                    <px:PXDropDown ID="edStatus2" runat="server" DataField="Status" />
                                    <px:PXDateTimeEdit ID="edPlannedStartDate2" runat="server" DataField="PlannedStartDate" />
                                    <px:PXDateTimeEdit ID="edPlannedEndDate2" runat="server" DataField="PlannedEndDate" />
                                    <px:PXDateTimeEdit ID="edStartDate2" runat="server" DataField="StartDate" />
                                    <px:PXDateTimeEdit ID="edEndDate2" runat="server" DataField="EndDate" />
                                    <px:PXSelector ID="edApproverID2" runat="server" DataField="ApproverID" AutoRefresh="True" />
                                    <px:PXSelector ID="edTaxCategoryID2" runat="server" DataField="TaxCategoryID" AutoRefresh="True" AllowEdit="True"/>
                                    <px:PXCheckBox ID="chkVisibleInGL2" runat="server" Checked="True" DataField="VisibleInGL" />
                                    <px:PXCheckBox ID="chkVisibleInAP2" runat="server" Checked="True" DataField="VisibleInAP" />
                                    <px:PXCheckBox ID="chkVisibleInAR2" runat="server" Checked="True" DataField="VisibleInAR" />
                                    <px:PXCheckBox ID="chkVisibleInSO2" runat="server" Checked="True" DataField="VisibleInSO" />
                                    <px:PXCheckBox ID="chkVisibleInPO" runat="server" Checked="True" DataField="VisibleInPO" />
                                    <px:PXCheckBox ID="chkVisibleInTA" runat="server" DataField="VisibleInTA" />
                                    <px:PXCheckBox ID="chkVisibleInEA" runat="server" DataField="VisibleInEA" />
                                    <px:PXCheckBox ID="chkVisibleInIN" runat="server" Checked="True" DataField="VisibleInIN" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaskCD" AutoCallBack="True" LinkCommand="ViewTask" />
                                    <px:PXGridColumn DataField="Type"/>
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="RateTableID" />
                                    <px:PXGridColumn DataField="AllocationID" />
                                    <px:PXGridColumn DataField="BillingID" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CompletedPercent" TextAlign="Right" />
                                    <px:PXGridColumn DataField="StartDate" />
                                    <px:PXGridColumn DataField="EndDate" />
                                    <px:PXGridColumn DataField="ApproverID" />
                                    <px:PXGridColumn DataField="BillingOption" Label="Billing Option" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
                                    <px:PXGridColumn DataField="IsDefault" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="BillSeparately" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="DefaultAccountID" />
									<px:PXGridColumn DataField="DefaultSubID" />
									<px:PXGridColumn DataField="PlannedStartDate" />
									<px:PXGridColumn DataField="PlannedEndDate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar >
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Common Tasks" PopupPanel="PanelAddTasks" />
                                <px:PXToolBarButton Key="cmdActivateTasks" Text="Activate Tasks">
                                    <AutoCallBack Command="ActivateTasks" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdCompleteTask" Text="Complete Task">
                                    <AutoCallBack Command="CompleteTasks" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="True" AllowUpload="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Revenue Budget" LoadOnDemand="False">
                <Template>
                    <px:PXFormView ID="RevenueFilter" runat="server" DataMember="RevenueFilter" RenderStyle="Normal" >
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                           <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="true" CommitChanges="true" />
                             <px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                           <px:PXCheckBox ID="chkGroupByTask" runat="server" DataField="GroupByTask" CommitChanges="true"/>
                            <px:PXLayoutRule ID="PXLayoutRule12" runat="server" ControlSize="L" LabelsWidth="M" StartColumn="True" />
                           <px:PXNumberEdit ID="edCuryAmountToInvoiceTotal" runat="server" DataField="CuryAmountToInvoiceTotal" Enabled="false" />

                            
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="RevenueBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True" OnRowDataBound="BudgetGrid_RowDataBound" AllowPaging="True" AdjustPageSize="None" PageSize="200">
                        <Levels>
                            <px:PXGridLevel DataMember="RevenueBudget">
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" LinkCommand ="ViewRevenueBudgetInventory"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Type" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DraftChangeOrderQty" TextAlign="Right"  />
                                    <px:PXGridColumn DataField="CuryDraftChangeOrderAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="MaxQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedQty" Label="Committed Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" Label="Committed Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedReceivedQty" Label="Committed Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedInvoicedQty" Label="Committed Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" Label="Committed Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedOpenQty" Label="Committed Open Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" Label="Committed Open Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryInvoicedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ActualQty" Label="Actual Qty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualAmount" Label="Actual Amount" TextAlign="Right" />
									<px:PXGridColumn DataField="ActualAmount" Label="Actual Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentInvoiced" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAvailable" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmountToInvoice" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
                                    <px:PXGridColumn DataField="RetainageMaxPct" />
                                    <px:PXGridColumn DataField="CuryCapAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDraftRetainedAmount" />
                                    <px:PXGridColumn DataField="CuryRetainedAmount" />
                                    <px:PXGridColumn DataField="CuryTotalRetainedAmount" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edCostCodeRevenue" runat="server" DataField="CostCodeID" AllowAddNew="true" />
                                    <px:PXSegmentMask ID="edInventoryIDRB" runat="server" DataField="InventoryID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdViewRevenueCommitments">
                                    <AutoCallBack Command="ViewRevenueCommitments" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="View Transactions" Key="cmdViewRevenueTransactions">
                                    <AutoCallBack Command="ViewRevenueTransactions" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="RevenueBudgetGrid" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Cost Budget" LoadOnDemand="False">
                <Template>
                     <px:PXFormView ID="CostFilter" runat="server" DataMember="CostFilter" RenderStyle="Normal">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                           <px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="true" CommitChanges="true" />
                             <px:PXLayoutRule ID="PXLayoutRule10" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                           <px:PXCheckBox ID="chkGroupByTask" runat="server" DataField="GroupByTask" CommitChanges="true"/>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="CostBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True" OnRowDataBound="BudgetGrid_RowDataBound" AllowPaging="True" AdjustPageSize="None" PageSize="200">
                        <Levels>
                            <px:PXGridLevel DataMember="CostBudget">
                                <RowTemplate>
                                    <px:PXSelector ID="edRevenueInventoryID" runat="server" DataField="RevenueInventoryID" AutoRefresh="true" />
                                    <px:PXSegmentMask ID="edCostCodeCost" runat="server" DataField="CostCodeID" AllowAddNew="true" />
                                    <px:PXSegmentMask ID="edInventoryIDCB" runat="server" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" LinkCommand ="ViewCostBudgetInventory"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Type" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" />
                                    <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DraftChangeOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDraftChangeOrderAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CommittedOrigQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedOrigAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedCOQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedReceivedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedInvoicedQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommittedOpenQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ActualQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="ActualAmount" Label="Actual Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" />
                                    <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryCostToComplete" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCostAtCompletion" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PercentCompleted" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryLastCostToComplete" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryLastCostAtCompletion" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastPercentCompleted" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RevenueTaskID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="RevenueInventoryID" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdViewCostCommitments">
                                    <AutoCallBack Command="ViewCostCommitments" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="View Transactions" Key="cmdViewCostTransactions">
                                    <AutoCallBack Command="ViewCostTransactions" Target="ds" />
                                    <PopupCommand Command="Refresh" Target="CostBudgetGrid" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Balances" LoadOnDemand="true">
                <Template>
                    <px:PXGrid runat="server" ID="ProjectBalanceGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="False" AllowSearch="False"
                        AllowFilter="False" OnRowDataBound="ProjectBalanceGrid_RowDataBound">
                        <Levels>
                            <px:PXGridLevel DataMember="BalanceRecords" DataKeyNames="RecordID">
                                <Columns>
                                    <px:PXGridColumn DataField="AccountGroup" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDraftCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryBudgetedCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryOriginalCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedCOAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" />
									<px:PXGridColumn DataField="ActualAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" />
                                </Columns>
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowFormEdit="False" AllowSort="False" AllowUpdate="False" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar DefaultAction="cmdViewBalance">
                            <Actions>
                                <AddNew ToolBarVisible="False" />
                                <Delete ToolBarVisible="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="Transactions" Key="cmdViewBalanceTransactions" Visible="False">
                                    <AutoCallBack Command="ViewBalanceTransactions" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                            <CustomItems>
                                <px:PXToolBarButton Text="Commitments" Key="cmdViewCommitments" Visible="False">
                                    <AutoCallBack Command="ViewCommitments" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Commitments" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="PurchaseOrdersGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds"
                        SkinID="Inquire" AllowFilter="true">
                        <Levels>
                            <px:PXGridLevel DataMember="PurchaseOrders">
                                <Columns>
                                    <px:PXGridColumn DataField="OrderType" />
                                    <px:PXGridColumn DataField="OrderNbr" LinkCommand="ViewPurchaseOrder" />
                                    <px:PXGridColumn DataField="OrderDate" />
                                    <px:PXGridColumn DataField="VendorID" />
                                    <px:PXGridColumn DataField="VendorID_Vendor_acctName" />
                                    <px:PXGridColumn DataField="OrderQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryID" />
                                    <px:PXGridColumn DataField="Status" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
						 <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdCreatePurchaseOrder">
                                    <AutoCallBack Command="CreatePurchaseOrder" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdCreateSubcontract">
                                    <AutoCallBack Command="CreateSubcontract" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Invoices" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="InvoicesGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds"
                        SkinID="Inquire" AllowFilter="true">
                        <Levels>
                            <px:PXGridLevel DataMember="Invoices">
                                <Columns>
                                    <px:PXGridColumn DataField="RecordNumber" />
                                    <px:PXGridColumn DataField="PMProforma__InvoiceDate" />
                                    <px:PXGridColumn DataField="ProformaRefNbr" LinkCommand="ViewProforma" />
                                    <px:PXGridColumn DataField="PMProforma__Description" Label="Description" />
                                    <px:PXGridColumn DataField="PMProforma__Status" />
                                    <px:PXGridColumn DataField="PMProforma__CuryDocTotal" />
									<px:PXGridColumn DataField="PMProforma__CuryID" />
                                    <px:PXGridColumn DataField="ARDocType" />
                                    <px:PXGridColumn DataField="ARRefNbr" LinkCommand="ViewInvoice"/>
                                    <px:PXGridColumn DataField="ARInvoice__DocDate" />                                    
                                    <px:PXGridColumn DataField="ARInvoice__DocDesc" Label="Description" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryOrigDocAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryRetainageTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryOrigDocAmtWithRetainageTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryDocBal" TextAlign="Right" />
									<px:PXGridColumn DataField="ARInvoice__CuryID" />
                                    <px:PXGridColumn DataField="ARInvoice__Status" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryRetainageUnreleasedAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ARInvoice__IsRetainageDocument" Type="CheckBox" />
                                    <px:PXGridColumn DataField="ARInvoice__OrigRefNbr" TextAlign="Right"  LinkCommand="ViewOrigDocument"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                         <ActionBar >
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdviewReleaseRetainage" Text="Release Retainage">
                                    <AutoCallBack Command="ViewReleaseRetainage" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
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
                                    <px:PXGridColumn DataField="ClassID" />
                                    <px:PXGridColumn DataField="ProjectNbr" />
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Date" />
                                    <px:PXGridColumn DataField="CompletionDate" />                                    
                                    <px:PXGridColumn DataField="DelayDays" />
                                    <px:PXGridColumn DataField="ExtRefNbr" />                                    
                                    <px:PXGridColumn DataField="RevenueTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CommitmentTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CostTotal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ReverseStatus" />
                                    <px:PXGridColumn DataField="OrigRefNbr" LinkCommand="ViewOrigChangeOrder" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
						<ActionBar >
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdCreateChangeOrder" DisplayStyle="Image" ImageKey="AddNew"  Tooltip="Create Change Order">
                                    <AutoCallBack Command="CreateChangeOrder" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Change Requests">
		<Template>
			<px:PXGrid runat="server" ID="gridChangeRequests" SkinID="Inquire" Width="100%" SyncPosition="True">
				<Levels>
					<px:PXGridLevel DataMember="ChangeRequests">
						<Columns>
							<px:PXGridColumn DataField="RefNbr" LinkCommand="ViewChangeRequest" />
							<px:PXGridColumn DataField="Status" />
							<px:PXGridColumn DataField="Date" />
							<px:PXGridColumn DataField="Description" />
							<px:PXGridColumn DataField="CostTotal" />
							<px:PXGridColumn DataField="LineTotal" />
							<px:PXGridColumn DataField="MarkupTotal" />
							<px:PXGridColumn DataField="PriceTotal" /></Columns></px:PXGridLevel></Levels>
				<AutoSize Enabled="True" MinHeight="150" />
				<Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" /></px:PXGrid></Template></px:PXTabItem>
            <px:PXTabItem Text="Union Locals" LoadOnDemand="true">
                <Template>
                    <px:PXGrid runat="server" ID="UnionsGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Unions" >
                                <Columns>
                                    <px:PXGridColumn DataField="UnionID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UnionID_Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activity History" LoadOnDemand="true">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Details" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray" PreviewPanelStyle="z-index: 100; background-color: Window"
                        PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar DefaultAction="cmdViewActivity" PagerVisible="False">
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds" />
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds" />
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds" />
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdViewActivity" Visible="false">
                                    <ActionBar MenuVisible="false" />
                                    <AutoCallBack Command="ViewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <RowTemplate>
                                    <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
                                    <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
                                    <px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
                                    <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="CRReminder__ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Summary" LinkCommand="ViewActivity" />
                                    <px:PXGridColumn DataField="ApprovalStatus" />
                                    <px:PXGridColumn DataField="Released" />
                                    <px:PXGridColumn DataField="StartDate" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeBillable" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" />
                        </CallbackCommands>
                        <AutoSize Enabled="True" />
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label">
                                <AutoSize Container="Parent" Enabled="true" />
                            </px:PXHtmlView>
                        </PreviewPanelTemplate>
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employees" LoadOnDemand="true">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="200" SkinID="Horizontal" Height="400px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridEmployeeContract" runat="server" DataSourceID="ds" SyncPosition="True" Height="400px" Width="100%"
                                SkinID="DetailsInTab">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeContract">
                                        <RowTemplate>
                                            <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="EmployeeID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="EPEmployee__AcctName" Label="Employee Name" />
                                            <px:PXGridColumn DataField="EPEmployee__DepartmentID" Label="Department ID" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoCallBack Target="gridContractRates" Command="Refresh" />
                                <Mode InitNewRow="True" />
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridContractRates" runat="server" DataSourceID="ds" Height="400px" Width="100%"
                                SkinID="DetailsInTab" Caption="Overrides" AllowPaging="False">
                                <CallbackCommands>
                                    <Refresh SelectControlsIDs="gridEmployeeContract" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="ContractRates">
                                        <Columns>
                                            <px:PXGridColumn DataField="EarningType" CommitChanges="True" />
                                            <px:PXGridColumn DataField="EPEarningType__Description" />
                                            <px:PXGridColumn DataField="LabourItemID" Label="Labor Item" CommitChanges="True"/>
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Equipment" LoadOnDemand="true">
                <Template>
                    <px:PXGrid runat="server" ID="ProjectRatesGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="EquipmentRates" DataKeyNames="EquipmentID,ProjectID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSelector ID="edEquipmentID" runat="server" DataField="EquipmentID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="EquipmentID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="EPEquipment__Description" />
                                    <px:PXGridColumn DataField="EPEquipment__RunRateItemID" />
                                    <px:PXGridColumn DataField="RunRate" />
                                    <px:PXGridColumn DataField="EPEquipment__SetupRateItemID" />
                                    <px:PXGridColumn DataField="SetupRate" />
                                    <px:PXGridColumn DataField="EPEquipment__SuspendRateItemID" />
                                    <px:PXGridColumn DataField="SuspendRate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True" GroupCaption="Default Values" />
                    <px:PXSegmentMask ID="edDefaultAccountID" runat="server" DataField="DefaultAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
                    <px:PXSegmentMask ID="edDefaultAccrualAccountID" runat="server" DataField="DefaultAccrualAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDefaultAccrualSubID" runat="server" DataField="DefaultAccrualSubID" />
                    <px:PXLabel ID="dummylable" runat="server"></px:PXLabel>
                    <px:PXGrid runat="server" ID="AccountTaskGrid" Height="220px" Caption="Default Task for GL Account" Width="400px" DataSourceID="ds" SkinID="DetailsWithFilter" AllowPaging="false" FilesIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Accounts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="true" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AccountID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TaskID" CommitChanges="True"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                    <px:PXLayoutRule runat="server" ID="PXLayoutRule1" StartColumn="True" StartGroup="True" />
                    <px:PXGrid runat="server" ID="gridMarkups" SyncPosition="True" FilesIndicator="False" AllowFilter="False" AllowSearch="False" Width="600" Height="220" Caption="Document Markups" CaptionVisible="True" AllowPaging="False" AdjustPageSize="None" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="Markups">
                                <RowTemplate>
                                  <px:PXTextEdit runat="server" ID="edMarkupDescription" DataField="Description" />
                                  <px:PXDropDown runat="server" ID="edMarkupType" DataField="Type" />
                                  <px:PXNumberEdit runat="server" ID="edMarkupValue" DataField="Value" />
                                  <px:PXSelector runat="server" ID="edMarkupTaskID" DataField="TaskID" />
                                  <px:PXSelector runat="server" ID="edMarkupAccountGroupID" DataField="AccountGroupID" />
                                  <px:PXSelector runat="server" ID="edMarkupCostCodeID" DataField="CostCodeID" />
                                  <px:PXSelector runat="server" ID="edMarkupInventoryID" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Value" />
                                    <px:PXGridColumn DataField="TaskID" />
                                    <px:PXGridColumn DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="CostCodeID" />
                                    <px:PXGridColumn DataField="InventoryID" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="False" />
                        <Mode AllowDragRows="True" AllowAddNew="True" AllowDelete="True" AllowSort="False" />
                        <ActionBar Position="Top" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Value" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="false" />
                                <EditRecord Enabled="false" />
                                <Delete Enabled="false" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
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
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings" Overflow="Hidden" LoadOnDemand="True">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px" Caption="Mailings"
                                AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds">
                                <AutoSize Enabled="True" />
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                            <px:PXDropDown ID="edFormat" runat="server" DataField="Format" />
                                            <px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID" />
                                            <px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
                                            <px:PXSelector ID="edSetupID" runat="server" DataField="SetupID" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="SetupID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch" />
                                            <px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="ReportID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients" AdjustPageSize="Auto"
                                AllowPaging="True" DataSourceID="ds">
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True"></Mode>
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" />
                                </Parameters>
                                <CallbackCommands>
                                    <Save RepaintControls="None" RepaintControlsIDs="ds" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
                                        <Mode InitNewRow="True"></Mode>
                                        <Columns>
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Email" />
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                            <px:PXDropDown ID="edContactType" runat="server" DataField="ContactType" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName"
                                                AllowEdit="True" />
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" ID="grdComplianceDocuments" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" AutoGenerateColumns="Append" DataSourceID="ds" AllowPaging="True" PageSize="12">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
                                <RowTemplate>
                                    <px:PXSegmentMask runat="server" DataField="CostCodeID" AutoRefresh="True" ID="edCostCode2" />
                                    <px:PXSelector runat="server" ID="edDocumentTypeValue" DataField="DocumentTypeValue" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edBillID" DataField="BillID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edInvoiceID" DataField="InvoiceID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edApCheckID" DataField="ApCheckID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edArPaymentID" DataField="ArPaymentID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edProjectTransactionID" DataField="ProjectTransactionID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" ID="edPurchaseOrder" DataField="PurchaseOrder" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" />
                                    <px:PXSelector runat="server" DataField="PurchaseOrderLineItem" AutoRefresh="True" ID="edPurchaseOrderLineItem" />
                                    <px:PXSelector runat="server" DataField="Subcontract" FilterByAllFields="True" AutoRefresh="True" CommitChanges="True" ID="edSubcontract" />
                                    <px:PXSelector runat="server" DataField="SubcontractLineItem" AutoRefresh="True" ID="edSubcontractLineItem" />
                                    <px:PXSelector runat="server" DataField="ChangeOrderNumber" AutoRefresh="True" ID="edChangeOrderNumber" />
                                    <px:PXSelector runat="server" ID="edProjectID" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ExpirationDate" TextAlign="Left" CommitChanges="True" />
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
                                    <px:PXGridColumn DataField="CustomerID" CommitChanges="True" LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                                    <px:PXGridColumn DataField="CustomerName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="VendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="VendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="BillID" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$BillID$Link" />
                                    <px:PXGridColumn DataField="BillAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountID" TextAlign="Left" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ApCheckID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ApCheckID$Link" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ArPaymentID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ArPaymentID$Link" />
                                    <px:PXGridColumn DataField="CertificateNumber" TextAlign="Left" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn DataField="DateIssued" TextAlign="Left" />
                                    <px:PXGridColumn DataField="EffectiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InsuranceCompany" TextAlign="Left" />
                                    <px:PXGridColumn DataField="InvoiceAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="InvoiceID" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$InvoiceID$Link" />
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
                                    <px:PXGridColumn DataField="ProjectTransactionID" DisplayMode="Text" TextAlign="Left" CommitChanges="True" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrder" LinkCommand="ComplianceDocument$PurchaseOrder$Link" DisplayMode="Text" CommitChanges="True" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" LinkCommand="ComplianceDocument$Subcontract$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn DataField="ReceiptDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceiveDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ReceivedBy" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn DataField="SecondaryVendorName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SourceType" TextAlign="Left" />
                                    <px:PXGridColumn DataField="SponsorOrganization" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ThroughDate" TextAlign="Left" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Lien Waiver Settings" LoadOnDemand="False" RepaintOnDemand="False">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp3" Panel1MinSize="90" Panel1Overflow="True" SplitterPosition="20" Height="200px" SkinID="Horizontal">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXLayoutRule runat="server" ID="PXLayoutRule2" StartColumn="True" ColumnWidth="XL" />
                            <px:PXLayoutRule runat="server" ID="PXLayoutRule4" StartGroup="True" GroupCaption="Conditional Lien Waivers" LabelsWidth="S" ControlSize="M" />
                            <px:PXDropDown runat="server" ID="PXDropDown6" DataField="ThroughDateSourceConditional" />
                            <px:PXCheckBox runat="server" DataField="IsActive" ID="PXCheckBox10" />
                            <px:PXLayoutRule runat="server" ID="PXLayoutRule3" StartColumn="True" ColumnWidth="XL" />
                            <px:PXLayoutRule runat="server" ID="PXLayoutRule5" StartGroup="True" GroupCaption="Unconditional Lien Waivers" LabelsWidth="S" ControlSize="M" />
                            <px:PXDropDown runat="server" ID="PXDropDown7" DataField="ThroughDateSourceUnconditional" />
                        </Template1>
                        <Template2>
                            <px:PXGrid runat="server" ID="ComplianceSettingsGrid" SyncPosition="True" Height="100%" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" AllowPaging="True" AdjustPageSize="None" PageSize="200">
                                <Levels>
                                    <px:PXGridLevel DataMember="LienWaiverRecipients">
                                        <Columns>
                                            <px:PXGridColumn DataField="VendorClassId" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="MinimumCommitmentAmount" AutoCallBack="True" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXNumberEdit runat="server" ID="PXNumberEdit2" DataField="MinimumCommitmentAmount" />
                                        </RowTemplate>
                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar>
                                    <CustomItems>
                                        <px:PXToolBarButton Key="cmdAddAllVendorClasses">
                                            <AutoCallBack Command="AddAllVendorClasses" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
     <px:PXSmartPanel ID="PanelTemplateSettings" runat="server" AcceptButtonID="PXButtonOK" AutoReload="true" CancelButtonID="PXButtonCancel" Caption="New Project Template"
        CaptionVisible="True" AutoSaveChanges="True" HideAfterAction="true" Key="TemplateSettings" LoadOnDemand="true">
        <px:PXFormView ID="formTemplateSettings" runat="server" CaptionVisible="False" DataMember="TemplateSettings" DataSourceID="ds" Style="z-index: 100" Width="100%"
            DefaultControlID="edTemplateID">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSegmentMask ID="edTemplateID" runat="server" DataField="TemplateID" CommitChanges="True" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="ChangeIDDialog">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
                <AutoCallBack Target="formChangeID" Command="Save" />
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelAddTasks" runat="server" Height="396px" Width="873px" Caption="Add Tasks" CaptionVisible="True" Key="TasksForAddition" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="gridAddTasks">
        <px:PXGrid ID="gridAddTasks" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Inquire" NoteIndicator="false" FilesIndicator="false">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="TasksForAddition">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
                        <px:PXGridColumn DataField="TaskCD" Label="Task ID" />
                        <px:PXGridColumn DataField="Description" Label="Description" />
                        <px:PXGridColumn DataField="ApproverID" />
                        <px:PXGridColumn DataField="PMProject__NonProject" Label="IS Global" TextAlign="Center" Type="CheckBox" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="Add " CommandName="AddTasks" CommandSourceID="ds" />
            <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelCopy" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Copy Project"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="CopyDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyProject"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK2" CancelButtonID="PXButtonCancel2" >
		<px:PXFormView ID="formCopyProject" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Settings" CaptionVisible="False" SkinID="Transparent"
			DataMember="CopyDialog">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" CommitChanges="True" />
			</Template>
		</px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButtonOK2" runat="server" Text="OK" DialogResult="Yes" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButtonCancel2" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
