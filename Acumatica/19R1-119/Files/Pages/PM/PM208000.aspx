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
                    <px:PXNumberEdit ID="edRetainagePct" runat="server" DataField="RetainagePct" />
                    
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
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
                    
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
                                    <px:PXGridColumn DataField="TaskCD" Width="81px" AutoCallBack="True" LinkCommand="ViewTask"/>
                                    <px:PXGridColumn DataField="Description" Width="200px" />
                                    <px:PXGridColumn DataField="RateTableID" Width="93px" />
                                    <px:PXGridColumn DataField="AllocationID" Width="117px" />
                                    <px:PXGridColumn DataField="BillingID" Width="117px" />
                                    <px:PXGridColumn DataField="ApproverID" Width="108px" />
                                    <px:PXGridColumn DataField="BillingOption" Label="Billing Option" RenderEditorText="True" Width="144px" />
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="117px" />
                                    <px:PXGridColumn DataField="IsDefault" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="BillSeparately" TextAlign="Center" Type="CheckBox" Width="120px" />
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="180px"/>
                                    <px:PXGridColumn DataField="Qty" Label="Qty." TextAlign="Right" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" Label="UOM" Width="63px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryUnitRate" Label="Rate" TextAlign="Right" Width="99px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryAmount" Label="Amount" TextAlign="Right" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="LimitQty" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="LimitAmount" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="PrepaymentPct" TextAlign="Right" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaymentAmount" TextAlign="Right" Width="81px"  CommitChanges="true"/>
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" Width="100px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="100px" />
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
                                    <px:PXGridColumn AutoCallBack="True" DataField="ProjectTaskID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="AccountGroupID" Width="108px" />
                                    <px:PXGridColumn DataField="Description" Width="180px"/>
                                    <px:PXGridColumn AutoCallBack="True" DataField="UOM" Width="63px" />
                                    <px:PXGridColumn DataField="CuryUnitRate" TextAlign="Right" Width="99px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="81px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="IsProduction" AutoCallBack="True" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="RevenueTaskID" Width="100px" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="RevenueInventoryID" Width="100px" AutoCallBack="True" />
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
                                SkinID="DetailsInTab" Caption="Overrides">
                                <CallbackCommands>
                                    <Refresh SelectControlsIDs="gridEmployeeContract" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="ContractRates" DataKeyNames="RecordID">
                                        <Columns>
                                            <px:PXGridColumn DataField="EarningType" Width="110px" CommitChanges="True" />
                                            <px:PXGridColumn DataField="EPEarningType__Description" Width="110px" />
                                            <px:PXGridColumn DataField="LabourItemID" Width="100px" Label="Labor Item" />
                                            <px:PXGridColumn DataField="InventoryItem__BasePrice" Width="200px" />
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
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="EquipmentID" Width="108px" />
                                    <px:PXGridColumn DataField="EPEquipment__Description" Width="200px" />
                                    <px:PXGridColumn DataField="EPEquipment__RunRateItemID" Width="90px" />
                                    <px:PXGridColumn DataField="RunRate" Width="90px" />
                                    <px:PXGridColumn DataField="EPEquipment__SetupRateItemID" Width="90px" />
                                    <px:PXGridColumn DataField="SetupRate" Width="90px" />
                                    <px:PXGridColumn DataField="EPEquipment__SuspendRateItemID" Width="90px" />
                                    <px:PXGridColumn DataField="SuspendRate" Width="90px" />
                                </Columns>
                                <Layout FormViewHeight="" />
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
                    <px:PXGrid runat="server" ID="AccountTaskGrid" Height="220px" Caption="Default Task for GL Account" Width="400px" DataSourceID="ds" SkinID="ShortList" AllowPaging="false" FilesIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Accounts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AccountID" Width="108px" CommitChanges="True" />
                                    <px:PXGridColumn DataField="TaskID" Width="108px" />
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
