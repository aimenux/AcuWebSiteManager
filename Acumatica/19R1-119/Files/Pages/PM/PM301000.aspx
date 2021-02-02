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
            <px:PXDSCallbackCommand DependOnGrid="RevenueBudgetGrid" Name="ViewRevenueTransactions" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="RevenueBudgetGrid" Name="ViewRevenueCommitments" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="ProjectBalanceGrid" Name="ViewBalanceTransactions" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="ProjectBalanceGrid" Name="ViewCommitments" Visible="False" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewOrigDocument" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewProforma" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="ViewChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Name="ViewOrigChangeOrder" Visible="False" DependOnGrid="ChangeOrdersGrid" />
            <px:PXDSCallbackCommand Name="ViewPurchaseOrder" Visible="False" DependOnGrid="PurchaseOrdersGrid" />
			<px:PXDSCallbackCommand Name="CreatePurchaseOrder" Visible="False" />

            <px:PXDSCallbackCommand Name="AddTasks" Visible="False" CommitChanges="True" PostData="Page" />
            <px:PXDSCallbackCommand Name="ActivateTasks" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CompleteTasks" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />

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
                <GridProperties FastFilterFields="Description" />
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
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXNumberEdit ID="edAsset" runat="server" DataField="CuryAsset" Enabled="False" />
            <px:PXNumberEdit ID="edLiability" runat="server" DataField="CuryLiability" Enabled="False" />
            <px:PXNumberEdit ID="edIncome" runat="server" DataField="CuryIncome" Enabled="False" />
            <px:PXNumberEdit ID="edExpense" runat="server" DataField="CuryExpense" Enabled="False" />
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
                    <px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" CommitChanges="true" />

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
                                    <px:PXSelector ID="edTaxCategoryID2" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
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
                                    <px:PXGridColumn DataField="TaskCD" Width="81px" AutoCallBack="True" LinkCommand="ViewTask" />
                                    <px:PXGridColumn DataField="Description" Width="200px" />
                                    <px:PXGridColumn DataField="RateTableID" Width="93px" />
                                    <px:PXGridColumn DataField="AllocationID" Width="117px" />
                                    <px:PXGridColumn DataField="BillingID" Width="117px" />
                                    <px:PXGridColumn DataField="Status" RenderEditorText="True" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CompletedPercent" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="StartDate" Width="90px" />
                                    <px:PXGridColumn DataField="EndDate" Width="90px" />
                                    <px:PXGridColumn DataField="ApproverID" Width="108px" />
                                    <px:PXGridColumn DataField="BillingOption" Label="Billing Option" RenderEditorText="True" Width="144px" />
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="117px" />
                                    <px:PXGridColumn DataField="IsDefault" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="BillSeparately" TextAlign="Center" Type="CheckBox" Width="120px" />
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="180px" />
                                    <px:PXGridColumn DataField="Type" TextAlign="Right" Width="81px"/>
                                    <px:PXGridColumn DataField="Qty" Label="Qty." TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" Label="UOM" Width="63px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryUnitRate" Label="Rate" TextAlign="Right" Width="99px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmount" Label="Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedQty" Label="Revised Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" Label="Revised Amount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" Label="Committed Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedReceivedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedInvoicedQty" Label="Committed Qty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" Label="Committed Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedOpenQty" Label="Committed Open Qty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" Label="Committed Open Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryInvoicedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="ActualQty" Label="Actual Qty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualAmount" Label="Actual Amount" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="ActualAmount" Label="Actual Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentInvoiced" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAvailable" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmountToInvoice" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="100px" />
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="180px" />
                                    <px:PXGridColumn DataField="Type" TextAlign="Right" Width="81px"/>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" Width="63px" />
                                    <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" Width="99px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="ChangeOrderQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryChangeOrderAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="RevisedQty" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" Width="81px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CommittedOrigQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedOrigAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedCOQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedCOAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedReceivedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedInvoicedQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CommittedOpenQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="ActualQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="ActualAmount" Label="Actual Amount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryCostToComplete" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCostAtCompletion" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PercentCompleted" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryLastCostToComplete" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryLastCostAtCompletion" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="LastPercentCompleted" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="RevenueTaskID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="RevenueInventoryID" Width="100px" AutoCallBack="True" />
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
                                    <px:PXGridColumn DataField="AccountGroup" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="500px" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryBudgetedCOAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryRevisedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryOriginalCommittedAmount" TextAlign="Right" Width="85px" />
                                    <px:PXGridColumn DataField="CuryCommittedCOAmount" TextAlign="Right" Width="85px" />
                                    <px:PXGridColumn DataField="CuryCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedInvoicedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualAmount" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="ActualAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryCommittedOpenAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryActualPlusOpenCommittedAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryVarianceAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="Performance" TextAlign="Right" Width="100px" />
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
                                    <px:PXGridColumn DataField="OrderType" Width="81px" />
                                    <px:PXGridColumn DataField="OrderNbr" Width="108px" LinkCommand="ViewPurchaseOrder" />
                                    <px:PXGridColumn DataField="OrderDate" Width="81px" />
                                    <px:PXGridColumn DataField="VendorID" Width="180px" />
                                    <px:PXGridColumn DataField="VendorID_Vendor_acctName" Width="150px" />
                                    <px:PXGridColumn DataField="OrderQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryID" Width="63px" />
                                    <px:PXGridColumn DataField="Status" Width="63px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
						 <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Key="cmdCreatePurchaseOrder">
                                    <AutoCallBack Command="CreatePurchaseOrder" Target="ds" />
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
                                    <px:PXGridColumn DataField="RecordNumber" Width="99px" />
                                    <px:PXGridColumn DataField="PMProforma__InvoiceDate" Width="99px" />
                                    <px:PXGridColumn DataField="ProformaRefNbr" Width="120px" LinkCommand="ViewProforma" />
                                    <px:PXGridColumn DataField="PMProforma__Description" Label="Description" Width="200px" />
                                    <px:PXGridColumn DataField="PMProforma__Status" Width="99px" />
                                    <px:PXGridColumn DataField="PMProforma__CuryDocTotal" Width="99px" />
									<px:PXGridColumn DataField="PMProforma__CuryID" Width="70px" />
                                    <px:PXGridColumn DataField="ARDocType" Width="99px" />
                                    <px:PXGridColumn DataField="ARRefNbr" Width="99px" LinkCommand="ViewInvoice"/>
                                    <px:PXGridColumn DataField="ARInvoice__DocDate" Width="99px" />                                    
                                    <px:PXGridColumn DataField="ARInvoice__DocDesc" Label="Description" Width="200px" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryOrigDocAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryRetainageTotal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryOrigDocAmtWithRetainageTotal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ARInvoice__CuryID" Width="70px" />
                                    <px:PXGridColumn DataField="ARInvoice__Status" Width="99px" />
                                    <px:PXGridColumn DataField="ARInvoice__CuryRetainageUnreleasedAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="ARInvoice__IsRetainageDocument" Width="100px" Type="CheckBox" />
                                    <px:PXGridColumn DataField="ARInvoice__OrigRefNbr" TextAlign="Right" Width="100px"  LinkCommand="ViewOrigDocument"/>
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
                                    <px:PXGridColumn DataField="RefNbr" Width="99px" LinkCommand="ViewChangeOrder"/>
                                    <px:PXGridColumn DataField="ClassID" Width="99px" />
                                    <px:PXGridColumn DataField="ProjectNbr" Width="99px" />
                                    <px:PXGridColumn DataField="Status" Width="99px" />
                                    <px:PXGridColumn DataField="Description" Width="120px" />
                                    <px:PXGridColumn DataField="Date" Width="63px" />
                                    <px:PXGridColumn DataField="CompletionDate" Width="80px" />                                    
                                    <px:PXGridColumn DataField="DelayDays" Width="80px" />
                                    <px:PXGridColumn DataField="ExtRefNbr" Width="80px" />                                    
                                    <px:PXGridColumn DataField="RevenueTotal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CommitmentTotal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CostTotal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="ReverseStatus"  Width="80px"/>
                                    <px:PXGridColumn DataField="OrigRefNbr" Width="100px" LinkCommand="ViewOrigChangeOrder" />
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
            <px:PXTabItem Text="Union Locals" LoadOnDemand="true">
                <Template>
                    <px:PXGrid runat="server" ID="UnionsGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Unions" >
                                <Columns>
                                    <px:PXGridColumn DataField="UnionID" Width="108px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UnionID_Description" Width="208px" />
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
                                <px:PXToolBarButton Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds" />
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
                                    <px:PXGridColumn DataField="Summary" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="ApprovalStatus" />
                                    <px:PXGridColumn DataField="Released" Width="80px" />
                                    <px:PXGridColumn DataField="StartDate" Width="108px" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="WorkgroupID" Width="90px" />
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
                                    <px:PXGridColumn DataField="ProjectID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="ProjectTaskID" Width="80px" AllowShowHide="true" Visible="false" SyncVisible="false" />
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
                                            <px:PXGridColumn DataField="EmployeeID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="EPEmployee__AcctName" Label="Employee Name" Width="216px" />
                                            <px:PXGridColumn DataField="EPEmployee__DepartmentID" Label="Department ID" Width="90px" />
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
                                            <px:PXGridColumn DataField="EarningType" Width="110px" CommitChanges="True" />
                                            <px:PXGridColumn DataField="EPEarningType__Description" Width="110px" />
                                            <px:PXGridColumn DataField="LabourItemID" Width="150px" Label="Labor Item" CommitChanges="True"/>
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
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="EquipmentID" Width="108px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="EPEquipment__Description" Width="200px" />
                                    <px:PXGridColumn DataField="EPEquipment__RunRateItemID" Width="90px" />
                                    <px:PXGridColumn DataField="RunRate" Width="90px" />
                                    <px:PXGridColumn DataField="EPEquipment__SetupRateItemID" Width="90px" />
                                    <px:PXGridColumn DataField="SetupRate" Width="90px" />
                                    <px:PXGridColumn DataField="EPEquipment__SuspendRateItemID" Width="105px" />
                                    <px:PXGridColumn DataField="SuspendRate" Width="90px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True" GroupCaption="Default Values" />
                    <px:PXSegmentMask ID="edDefaultAccountID" runat="server" DataField="DefaultAccountID" />
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
                    <px:PXSegmentMask ID="edDefaultAccrualAccountID" runat="server" DataField="DefaultAccrualAccountID" />
                    <px:PXSegmentMask ID="edDefaultAccrualSubID" runat="server" DataField="DefaultAccrualSubID" />
                    <px:PXLabel ID="dummylable" runat="server"></px:PXLabel>
                    <px:PXGrid runat="server" ID="AccountTaskGrid" Height="220px" Caption="Default Task for GL Account" Width="400px" DataSourceID="ds" SkinID="DetailsWithFilter" AllowPaging="false" FilesIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Accounts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AccountID" Width="108px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TaskID" Width="108px" CommitChanges="True"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="220px" AllowShowHide="False" TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
                                    <px:PXGridColumn DataField="Value" Width="148px" RenderEditorText="True" />
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
                                            <px:PXGridColumn DataField="SetupID" Width="108px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch" />
                                            <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="ReportID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Format" Width="54px" RenderEditorText="True" AutoCallBack="True" />
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
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID" Width="200px">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Email" Width="200px" />
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                            <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
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
                        <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                        <px:PXGridColumn DataField="TaskCD" Label="Task ID" />
                        <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                        <px:PXGridColumn DataField="ApproverID" />
                        <px:PXGridColumn DataField="PMProject__NonProject" Label="IS Global" TextAlign="Center" Type="CheckBox" Width="60px" />
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