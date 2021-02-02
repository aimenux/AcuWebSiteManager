<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM302000.aspx.cs" Inherits="Page_PM302000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Task" TypeName="PX.Objects.PM.ProjectTaskEntry" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Delete" PopupVisible="true" ClosePopup="true" />
            <px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewBalance" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewCommitments" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewTransactions" Visible="False" />
            <px:PXDSCallbackCommand Name="AutoBudget" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
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
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Task" LinkPage="" Caption="Task Summary" FilesIndicator="True"
        NoteIndicator="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" AutoRefresh="True" AllowAddNew="True" AllowEdit="True">
            </px:PXSegmentMask>
            <px:PXSegmentMask ID="edTaskCD" runat="server" DataField="TaskCD" AutoRefresh="True" DataSourceID="ds">
            </px:PXSegmentMask>
            <px:PXDropDown runat="server" ID="edType" DataField="Type" />
            
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="IsDefault" runat="server" DataField="IsDefault" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="341px" DataSourceID="ds" DataMember="TaskProperties" LinkPage="">
        <Items>
            <px:PXTabItem Text="Summary">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Task Properties" />
                    <px:PXDateTimeEdit ID="edPlannedStartDate" runat="server" DataField="PlannedStartDate" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edPlannedEndDate" runat="server" DataField="PlannedEndDate" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="True" />
                    <px:PXDropDown ID="edCompletedPctMethod" runat="server" DataField="CompletedPctMethod" CommitChanges="True" />
                    <px:PXNumberEdit ID="edCompletedPercent" runat="server" DataField="CompletedPercent" />
                    <px:PXSelector ID="edApproverID" runat="server" DataField="ApproverID" AutoRefresh="True" />
                    
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Billing And Allocation Settings" />
                    <px:PXCheckBox ID="chkBillSeparately" runat="server" DataField="BillSeparately" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" />                   
                    <px:PXSelector CommitChanges="True" ID="edAllocationID" runat="server" DataField="AllocationID" />
                    <px:PXSelector CommitChanges="True" ID="edBillingID" runat="server" DataField="BillingID" />
                    <px:PXSelector ID="edDefaultBranchID" runat="server" DataField="DefaultBranchID" />
                     <px:PXSelector ID="edRateTableID" runat="server" DataField="RateTableID" DataSourceID="ds" />
                    <px:PXDropDown ID="edBillingOption" runat="server" DataField="BillingOption" />
                    <px:PXSelector ID="edWipAccountGroupID" runat="server" DataField="WipAccountGroupID" />
                    

                    
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Values" />
                    
                    <px:PXSegmentMask ID="edDefaultAccountID" runat="server" DataField="DefaultAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" />
                     <px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="DefaultAccrualAccountID" CommitChanges="true" />
                    <px:PXSegmentMask ID="PXSegmentMask2" runat="server" DataField="DefaultAccrualSubID" />
                     <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" />
                   
                    
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Visibility Settings" />
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
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkVisibleInTA" runat="server" DataField="VisibleInTA" />
                    <px:PXCheckBox ID="chkVisibleInEA" runat="server" DataField="VisibleInEA" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" />

					<px:PXLayoutRule runat="server" GroupCaption="CRM" StartGroup="True" />
					<px:PXFormView ID="formA" runat="server" DataMember="TaskCampaign" DataSourceID="ds" SkinID="Transparent" TabIndex="2500">
						<Template>
							<px:PXSelector ID="edCampaignID" runat="server" Enabled="false" AllowEdit="True" DataField="CampaignID" TextMode="Search" DataSourceID="ds" LabelWidth="150" />
						</Template>
					</px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Recurring Billing">
                <Template>
                    <px:PXGrid ID="GridBillingItems" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="BillingItems" >
                                <RowTemplate>
                                    <px:PXSegmentMask Size="s" ID="edSubMask" runat="server" DataField="SubMask" DataMember="_PMRECBILL_Segments_" />
                                    <px:PXSegmentMask Size="s" ID="edSubID" runat="server" DataField="SubID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="AccountSource" RenderEditorText="True" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubMask" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="BranchId" />
                                    <px:PXGridColumn DataField="AccountID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubID" />
                                    <px:PXGridColumn DataField="ResetUsage" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Included" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UOM" />
                                    <px:PXGridColumn DataField="Used" TextAlign="Right" />
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
                                    <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <CallbackCommands>
                            <Refresh CommitChanges="True" PostData="Page" />
                        </CallbackCommands>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
                                      <AutoSize Container="Parent" Enabled="true" />
                                </px:PXHtmlView>
                        </PreviewPanelTemplate>
                    </pxa:PXGridWithPreview>
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
            <px:PXTabItem Text="Compliance">
                <Template>
                    <px:PXGrid runat="server" SyncPosition="True" KeepPosition="True" Height="300px" SkinID="DetailsInTab" Width="100%" ID="grdComplianceDocuments" AutoGenerateColumns="Append" DataSourceID="ds" AllowPaging="True" PageSize="12">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ComplianceDocuments">
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
                                    <px:PXSelector runat="server" DataField="ProjectID" FilterByAllFields="True" AutoRefresh="True" ID="edProjectID" />
                                    <px:PXSelector runat="server" DataField="CostTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edCostTaskID" />
                                    <px:PXSelector runat="server" DataField="RevenueTaskID" FilterByAllFields="True" AutoRefresh="True" ID="edRevenueTaskID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn TextAlign="Left" DataField="ExpirationDate" CommitChanges="True" />
                                    <px:PXGridColumn DataField="DocumentType" CommitChanges="True" />
                                    <px:PXGridColumn TextAlign="Left" DataField="CreationDate" />
                                    <px:PXGridColumn DataField="Status" CommitChanges="True" />
                                    <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="Required" />
                                    <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="Received" />
                                    <px:PXGridColumn TextAlign="Left" DataField="ReceivedDate" />
                                    <px:PXGridColumn DataField="IsProcessed" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsVoided" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="IsCreatedAutomatically" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn TextAlign="Left" DataField="SentDate" />
                                    <px:PXGridColumn DataField="ProjectID" CommitChanges="True" LinkCommand="ComplianceDocuments_Project_ViewDetails" />
                                    <px:PXGridColumn DataField="CostTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="RevenueTaskID" TextAlign="Left" LinkCommand="ComplianceDocuments_Task_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn TextAlign="Left" DataField="CostCodeID" LinkCommand="ComplianceDocuments_CostCode_ViewDetails" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CustomerID" CommitChanges="True" LinkCommand="ComplianceDocuments_Customer_ViewDetails" />
                                    <px:PXGridColumn TextAlign="Left" DataField="CustomerName" />
                                    <px:PXGridColumn DataField="VendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn TextAlign="Left" DataField="VendorName" />
                                    <px:PXGridColumn DisplayMode="Text" DataField="BillID" CommitChanges="True" LinkCommand="ComplianceDocument$BillID$Link" />
                                    <px:PXGridColumn TextAlign="Right" DataField="BillAmount" />
                                    <px:PXGridColumn TextAlign="Left" DataField="AccountID" />
                                    <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ApCheckID" CommitChanges="True" LinkCommand="ComplianceDocument$ApCheckID$Link" />
                                    <px:PXGridColumn DataField="CheckNumber" TextAlign="Left" />
                                    <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ArPaymentID" CommitChanges="True" LinkCommand="ComplianceDocument$ArPaymentID$Link" />
                                    <px:PXGridColumn TextAlign="Left" DataField="CertificateNumber" />
                                    <px:PXGridColumn DataField="CreatedByID" />
                                    <px:PXGridColumn TextAlign="Left" DataField="DateIssued" />
                                    <px:PXGridColumn TextAlign="Left" DataField="EffectiveDate" />
                                    <px:PXGridColumn TextAlign="Left" DataField="InsuranceCompany" />
                                    <px:PXGridColumn TextAlign="Right" DataField="InvoiceAmount" />
                                    <px:PXGridColumn DisplayMode="Text" DataField="InvoiceID" CommitChanges="True" LinkCommand="ComplianceDocument$InvoiceID$Link" />
                                    <px:PXGridColumn DataField="IsExpired" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsRequiredJointCheck" />
                                    <px:PXGridColumn TextAlign="Right" DataField="JointAmount" />
                                    <px:PXGridColumn TextAlign="Left" DataField="JointRelease" />
                                    <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="JointReleaseReceived" />
                                    <px:PXGridColumn DataField="JointVendorInternalId" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" TextAlign="Left" />
                                    <px:PXGridColumn DataField="JointVendorExternalName" TextAlign="Left" />
                                    <px:PXGridColumn DataField="LastModifiedByID" />
                                    <px:PXGridColumn TextAlign="Right" DataField="LienWaiverAmount" />
                                    <px:PXGridColumn TextAlign="Right" DataField="Limit" />
                                    <px:PXGridColumn TextAlign="Left" DataField="MethodSent" />
                                    <px:PXGridColumn TextAlign="Left" DataField="PaymentDate" />
                                    <px:PXGridColumn DataField="ArPaymentMethodID" />
                                    <px:PXGridColumn DataField="ApPaymentMethodID" />
                                    <px:PXGridColumn TextAlign="Left" DataField="Policy" />
                                    <px:PXGridColumn DisplayMode="Text" TextAlign="Left" DataField="ProjectTransactionID" CommitChanges="True" LinkCommand="ComplianceDocument$ProjectTransactionID$Link" />
                                    <px:PXGridColumn DataField="PurchaseOrderLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="Subcontract" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$Subcontract$Link" />
                                    <px:PXGridColumn DataField="SubcontractLineItem" TextAlign="Left" />
                                    <px:PXGridColumn DataField="ChangeOrderNumber" DisplayMode="Text" LinkCommand="ComplianceDocument$ChangeOrderNumber$Link" CommitChanges="True" />
                                    <px:PXGridColumn TextAlign="Left" DataField="PurchaseOrder" DisplayMode="Text" CommitChanges="True" LinkCommand="ComplianceDocument$PurchaseOrder$Link" />
                                    <px:PXGridColumn TextAlign="Left" DataField="ReceiveDate" />
                                    <px:PXGridColumn TextAlign="Left" DataField="ReceivedBy" />
                                    <px:PXGridColumn DataField="SecondaryVendorID" CommitChanges="True" LinkCommand="ComplianceDocuments_Vendor_ViewDetails" />
                                    <px:PXGridColumn TextAlign="Left" DataField="SecondaryVendorName" />
                                    <px:PXGridColumn TextAlign="Left" DataField="SourceType" />
                                    <px:PXGridColumn TextAlign="Left" DataField="SponsorOrganization" />
                                    <px:PXGridColumn TextAlign="Left" DataField="ThroughDate" />
                                    <px:PXGridColumn DataField="DocumentTypeValue" CommitChanges="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
