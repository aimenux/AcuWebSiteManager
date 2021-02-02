<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM208000.aspx.cs" Inherits="Page_PM208000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.TemplateMaint" PrimaryView="Project" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand DependOnGrid="TaskGrid" Name="ViewTask" Visible="False" />
            <px:PXDSCallbackCommand Name="AddAllVendorClasses" Visible="False" />
            <px:PXDSCallbackCommand Name="UpdateRetainage" Visible="False" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Project" Caption="Template Summary" FilesIndicator="True"
        NoteIndicator="True" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
            <px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" DataSourceID="ds" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" />
            <px:PXLayoutRule runat="server" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXCheckBox ID="chkNonProject" runat="server" DataField="NonProject" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="ProjectProperties">
        <Items>
            <px:PXTabItem Text="Summary">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Project Properties" />
                    <px:PXDropDown ID="edBudgetLevel" runat="server" DataField="BudgetLevel" CommitChanges="True" />
                    <px:PXDropDown ID="edCostBudgetLevel" runat="server" DataField="CostBudgetLevel" CommitChanges="True" />
                    <px:PXSelector ID="edApprover" runat="server" DataField="ApproverID" />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="S" CommitChanges="True" />
                    <px:PXCheckBox ID="edChangeOrderWorkflow" runat="server" DataField="ChangeOrderWorkflow" />
                    <px:PXCheckBox ID="chkRestrictToEmployeeList" runat="server" DataField="RestrictToEmployeeList" />
                    <px:PXCheckBox ID="chkRestrictToResourceList" runat="server" DataField="RestrictToResourceList" />
                    <px:PXCheckBox ID="edBudgetMetrics" runat="server" DataField="BudgetMetricsEnabled" />

                    <px:PXLayoutRule runat="server" GroupCaption="Billing And Allocation Settings" StartGroup="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXFormView ID="billingForm" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" Size="S" />
                        </Template>
                    </px:PXFormView>
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
                     
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" StartColumn="true" GroupCaption="Visibility Settings" />
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
                    <px:PXLayoutRule runat="server" ID="retainageRule1" StartGroup="True" GroupCaption="RETAINAGE" />
                    <px:PXDropDown runat="server" ID="edRetainageMode" DataField="RetainageMode" CommitChanges="True" />
                    <px:PXCheckBox runat="server" ID="edIncludeCO" DataField="IncludeCO" CommitChanges="True" Size="S" />
                    <px:PXLayoutRule runat="server" ID="retainageRule2" Merge="False" />

                    <px:PXLayoutRule runat="server" ID="retainageRule3" Merge="True" />
                    <px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" CommitChanges="True"></px:PXNumberEdit>
                    <px:PXCheckBox runat="server" ID="edSteppedRetainage" DataField="SteppedRetainage" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" ID="retainageRule3x" Merge="False" />
                    <px:PXNumberEdit runat="server" ID="edRetainageMaxPct" DataField="RetainageMaxPct" />
                    <px:PXGrid runat="server" ID="gridRetainageSteps" SyncPosition="True" FilesIndicator="False" AllowFilter="False" AllowSearch="False" Width="400" Height="120" Caption="Stepped Retainage" CaptionVisible="True" AllowPaging="False" AdjustPageSize="None" NoteIndicator="False">
                        <Levels>
                            <px:PXGridLevel DataMember="RetainageSteps">
                                <RowTemplate>
                                    <px:PXTextEdit runat="server" ID="edStepThresholdPct" DataField="ThresholdPct" />
                                    <px:PXTextEdit runat="server" ID="edStepRetainagePct" DataField="RetainagePct" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ThresholdPct" Width="180" />
                                    <px:PXGridColumn DataField="RetainagePct" Width="180" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="False" />
                        <Mode AllowAddNew="True" AllowDelete="True" AllowSort="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tasks">
                <Template>
                    <px:PXGrid runat="server" ID="TaskGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="Tasks" DataKeyNames="ProjectID,TaskCD">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edProjectTaskCD2" runat="server" DataField="TaskCD"/>
                                    <px:PXSegmentMask Size="s" ID="edDefaultSubID2" runat="server" DataField="DefaultSubID" />
                                    <px:PXSelector ID="edRateTableID2" runat="server" DataField="RateTableID" />
                                    <px:PXSelector Size="s" ID="edAllocationID2" runat="server" DataField="AllocationID" AllowAddNew="True" AllowEdit="True" />
                                    <px:PXSelector Size="s" ID="edBillingID2" runat="server" DataField="BillingID" AllowAddNew="True" AllowEdit="True" />
                                    <px:PXDropDown Size="m" ID="edBillingOption2" runat="server" DataField="BillingOption" />
                                    <px:PXSegmentMask ID="edDefaultAccountID2" runat="server" DataField="DefaultAccountID"  />
                                    <px:PXTextEdit ID="edDescription2" runat="server" DataField="Description" />
                                    <px:PXSelector ID="edApproverID2" runat="server" DataField="ApproverID" AutoRefresh="True" />
                                    <px:PXSelector ID="edTaxCategoryID2" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
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
                                    <px:PXGridColumn DataField="TaskCD" AutoCallBack="True" LinkCommand="ViewTask"/>
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="RateTableID" />
                                    <px:PXGridColumn DataField="AllocationID" />
                                    <px:PXGridColumn DataField="BillingID" />
                                    <px:PXGridColumn DataField="ApproverID" />
                                    <px:PXGridColumn DataField="BillingOption" Label="Billing Option" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
                                    <px:PXGridColumn DataField="IsDefault" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="BillSeparately" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
             <px:PXTabItem Text="Revenue Budget" LoadOnDemand="False">
                <Template>
                    <px:PXGrid ID="RevenueBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="RevenueBudget">
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description"/>
                                    <px:PXGridColumn DataField="Qty" Label="Qty." TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" Label="UOM" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryUnitRate" Label="Rate" TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryAmount" Label="Amount" TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="MaxQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right"  CommitChanges="true"/>
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" CommitChanges="true" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edCostCodeRevenue" runat="server" DataField="CostCodeID" AllowAddNew="true" />
                                    <px:PXSegmentMask runat="server" ID="edInventoryIDRB" DataField="InventoryID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Cost Budget" LoadOnDemand="False">
                <Template>
                    <px:PXGrid ID="CostBudgetGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="CostBudget">
                                <RowTemplate>
		                            <px:PXSelector ID="edRevenueInventoryID" runat="server" DataField="RevenueInventoryID" AutoRefresh="true"/>
                                     <px:PXSegmentMask ID="edCostCodeCost" runat="server" DataField="CostCodeID" AllowAddNew="true" />
                                    <px:PXSegmentMask runat="server" ID="edInventoryIDCB" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" />
                                    <px:PXGridColumn DataField="Description"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" />
                                    <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="RevenueTaskID" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="RevenueInventoryID" AutoCallBack="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Employees" BindingContext="form" VisibleExp="DataControls[&quot;chkNonProject&quot;].Value != true">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="200" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridEmployeeContract" runat="server" DataSourceID="ds" SyncPosition="True" Height="300px" Width="100%"
                                SkinID="DetailsInTab">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeContract">
                                        <RowTemplate>
		                                     <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID"/>
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
                                SkinID="DetailsInTab" Caption="Overrides">
                                <CallbackCommands>
                                    <Refresh SelectControlsIDs="gridEmployeeContract" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="ContractRates" DataKeyNames="RecordID">
                                        <Columns>
                                            <px:PXGridColumn DataField="EarningType" CommitChanges="True" />
                                            <px:PXGridColumn DataField="EPEarningType__Description" />
                                            <px:PXGridColumn DataField="LabourItemID" Label="Labor Item" />
                                            <px:PXGridColumn DataField="InventoryItem__BasePrice" />
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
            <px:PXTabItem Text="Equipment" BindingContext="form" VisibleExp="DataControls[&quot;chkNonProject&quot;].Value != true">
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
                                    <px:PXGridColumn DataField="EquipmentID" />
                                    <px:PXGridColumn DataField="EPEquipment__Description" />
                                    <px:PXGridColumn DataField="EPEquipment__RunRateItemID" />
                                    <px:PXGridColumn DataField="RunRate" />
                                    <px:PXGridColumn DataField="EPEquipment__SetupRateItemID" />
                                    <px:PXGridColumn DataField="SetupRate" />
                                    <px:PXGridColumn DataField="EPEquipment__SuspendRateItemID" />
                                    <px:PXGridColumn DataField="SuspendRate" />
                                </Columns>
                                <Layout FormViewHeight="" />
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
                    <px:PXSegmentMask ID="edDefaultAccountID" runat="server" DataField="DefaultAccountID" />
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
                    <px:PXSegmentMask ID="edDefaultAccrualAccountID" runat="server" DataField="DefaultAccrualAccountID" />
                    <px:PXSegmentMask ID="edDefaultAccrualSubID" runat="server" DataField="DefaultAccrualSubID" />
                    <px:PXLabel ID="dummylable" runat="server"></px:PXLabel>
                    <px:PXGrid runat="server" ID="AccountTaskGrid" Height="220px" Caption="Default Task for GL Account" Width="400px" DataSourceID="ds" SkinID="ShortList" AllowPaging="false" FilesIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Accounts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AccountID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TaskID" />
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
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXSmartPanel ID="PanelCopy" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="Copy Template"
		CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="CopyDialog" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyProject" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
		<px:PXFormView ID="formCopyProject" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Settings" CaptionVisible="False" SkinID="Transparent"
			DataMember="CopyDialog">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXSegmentMask ID="edTemplateID" runat="server" DataField="TemplateID" CommitChanges="True" />
			</Template>
		</px:PXFormView>
		<div style="padding: 5px; text-align: right;">
			<px:PXButton ID="PXButtonOK" runat="server" Text="OK" DialogResult="Yes" Width="63px" Height="20px"></px:PXButton>
			<px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
		</div>
	</px:PXSmartPanel>
</asp:Content>
