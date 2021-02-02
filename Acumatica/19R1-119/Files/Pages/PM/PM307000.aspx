<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM307000.aspx.cs" Inherits="Page_PM307000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ProformaEntry" PrimaryView="Document" BorderStyle="NotSet" PageLoadBehavior="GoLastRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" CommitChanges="true" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" />

            <px:PXDSCallbackCommand Name="AutoApplyPrepayments" Visible="false" />
            <px:PXDSCallbackCommand Name="RecalcExternalTax" Visible="false" />
            <px:PXDSCallbackCommand Name="AppendSelected" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="UploadUnbilled" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="Send" Visible="false" />
            <px:PXDSCallbackCommand Name="ProgressPasteLine" Visible="False" />
            <px:PXDSCallbackCommand Name="TransactPasteLine" Visible="False" />
            <px:PXDSCallbackCommand Name="ProgressResetOrder" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="TransactResetOrder" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Proforma Summary" FilesIndicator="True"
        NoteIndicator="True" NotifyIndicator="true" ActivityIndicator="True" ActivityField="NoteActivity" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S"/>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="true" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true" />
            
            <px:PXDateTimeEdit ID="edInvoiceDate" runat="server" DataField="InvoiceDate" CommitChanges="true"/>
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges="true" AutoRefresh="True" />

            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM"/>
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" AutoRefresh="True" AllowAddNew="True" AllowEdit="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" AutoRefresh="True" AllowAddNew="True" AllowEdit="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" DataSourceID="ds" RateTypeView="_PMProforma_CurrencyInfo_"
                DataMember="_Currency_"></pxa:PXCurrencyRate>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S"/>

            <px:PXNumberEdit ID="edCuryProgressiveTotal" runat="server" DataField="CuryProgressiveTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryTransactionalTotal" runat="server" DataField="CuryTransactionalTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryTaxTotalWithRetainage" runat="server" DataField="CuryTaxTotalWithRetainage" Enabled="False"  />
            <px:PXNumberEdit ID="edDocTotal" runat="server" DataField="CuryDocTotal" Enabled="False" />
            <px:PXNumberEdit ID="edOverflow" runat="server" DataField="Overflow.CuryOverflowTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryRetainageTotal" runat="server" DataField="CuryRetainageTotal" Enabled="False"  />
            <px:PXNumberEdit ID="edCuryAmountDue" runat="server" DataField="CuryAmountDue" Enabled="False"  />
        </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="DetailsPanel" runat="server" Height="396px" Width="850px" Caption="Transaction Details" CaptionVisible="True" Key="Details" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="TransactionLinesGrid" LoadOnDemand="true" AutoRepaint="true">
        <px:PXGrid ID="DetailsGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="Details">
                    <Columns>
                        <px:PXGridColumn DataField="RefNbr" Width="100px" LinkCommand="ViewTranDocument"/>
                        <px:PXGridColumn DataField="InventoryID" Width="90px" />
                        <px:PXGridColumn DataField="Description" Width="100px" />
                        <px:PXGridColumn DataField="ResourceID" Width="90px" />
                        <px:PXGridColumn DataField="BAccountID" Width="90px" />                        
                        <px:PXGridColumn DataField="Date" Width="90px" />
                        <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" Width="64px" />
                        <px:PXGridColumn DataField="Qty" Label="Qty" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="UOM" Width="90px" />
                        <px:PXGridColumn DataField="ProjectCuryAmount" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="InvoicedQty" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="ProjectCuryInvoicedAmount" TextAlign="Right" Width="90px" />
						<px:PXGridColumn DataField="ProjectCuryID" Width="50px" />
                    </Columns>
                    <RowTemplate>                                    
                        <px:PXSegmentMask runat="server" ID="edInventoryIDDtl" DataField="InventoryID" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False" />
                    <Delete MenuVisible="True" ToolBarVisible="Top" />
                    <NoteShow MenuVisible="False" ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="True" AllowUpdate="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="AppendUnbilledPanel" runat="server" Height="396px" Width="850px" Caption="Upload Unbilled Transactions" CaptionVisible="True" Key="Unbilled" AutoCallBack-Command="Refresh"
        AutoCallBack-Enabled="True" AutoCallBack-Target="UnbilledGrid" LoadOnDemand="true" AutoRepaint="true">
        <px:PXGrid ID="UnbilledGrid" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Details" SyncPosition="true">
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="Unbilled">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Label="Selected" Width="63px" Type="CheckBox" AllowCheckAll="true" />
                        <px:PXGridColumn DataField="BranchID" Label="Branch" Width="63px" />
                        <px:PXGridColumn DataField="RefNbr" Width="100px" LinkCommand="ViewTranDocument"/>
                        <px:PXGridColumn DataField="InventoryID" Width="90px" />
                        <px:PXGridColumn DataField="Description" Width="100px" />
                        <px:PXGridColumn DataField="ResourceID" Width="90px" />
                        <px:PXGridColumn DataField="BAccountID" Width="90px" />                        
                        <px:PXGridColumn DataField="Date" Width="90px" />
                        <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" Width="64px" />
                        <px:PXGridColumn DataField="Qty" Label="Qty" TextAlign="Right" Width="90px" />
                        <px:PXGridColumn DataField="UOM" Width="90px" />
                        <px:PXGridColumn DataField="BillableQty" Label="Billable Qty" TextAlign="Right" Width="54px" />
                        <px:PXGridColumn DataField="TranCuryUnitRate" Label="Unit Rate" TextAlign="Right" Width="54px" />
                        <px:PXGridColumn DataField="TranCuryAmount" TextAlign="Right" Width="90px" />
						<px:PXGridColumn DataField="TranCuryID" Width="50px" />
                        <px:PXGridColumn DataField="AccountGroupID" Label="Account Group" Width="90px" />
                        <px:PXGridColumn DataField="AccountID" Label="Account" Width="54px" />
                        <px:PXGridColumn DataField="SubID" Label="Subaccount" Width="108px" />
                        <px:PXGridColumn DataField="OffsetAccountID" Label="Offset Account" Width="54px" />
                        <px:PXGridColumn DataField="OffsetSubID" Label="Offset SubAccount" Width="108px" />
                    </Columns>
                    <RowTemplate>                                    
                        <px:PXSegmentMask runat="server" ID="edInventoryIDUbl" DataField="InventoryID" />
                    </RowTemplate>
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <AddNew MenuVisible="False" ToolBarVisible="False" />
                    <Delete MenuVisible="True" ToolBarVisible="Top" />
                    <NoteShow MenuVisible="False" ToolBarVisible="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" AllowDelete="True" AllowUpdate="False" />
        </px:PXGrid>
         <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd" runat="server" Text="Upload" CommandName="AppendSelected"  CommandSourceID="ds" />
            <px:PXButton ID="PXButtonAddClose" runat="server" Text="Upload & Close" DialogResult="OK"  />
            <px:PXButton ID="PXButtonClose" runat="server" DialogResult="Cancel" Text="Close" />      
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="DocumentSettings">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items >
            <px:PXTabItem Text="Progress Billing">
                <Template>
                    <px:PXGrid ID="ProgressiveLinesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ProgressiveLines">
                                <Columns>
                                    <px:PXGridColumn AutoCallBack="True" DataField="BranchID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="TaskID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="Description" Width="180px" />
                                    <px:PXGridColumn DataField="PMRevenueBudget__CuryRevisedAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="PMRevenueBudget__CuryActualAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="PMRevenueBudget__CuryInvoicedAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryPreviouslyInvoiced" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CompletedPct" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryMaterialStoredAmount" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryPrepaidAmount" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryLineTotal" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CurrentInvoicedPct" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryRetainage" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="100px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="AccountID" Width="100px" />
                                    <px:PXGridColumn DataField="SubID" Width="100px" />
                                    <px:PXGridColumn DataField="DefCode" Width="100px" />
                                    <px:PXGridColumn DataField="SortOrder" Width="63px" />
                                    <px:PXGridColumn DataField="LineNbr" Width="63px" />
                                </Columns>
                                <RowTemplate>                                    
                                    <px:PXSegmentMask runat="server" ID="edInventoryIDPL" DataField="InventoryID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="false" AllowDragRows="true" />
                        <CallbackCommands PasteCommand="ProgressPasteLine">
                            <%--<Save PostData="Container" />--%>
                        </CallbackCommands>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Time and Material">
                <Template>
                    <px:PXGrid ID="TransactionLinesGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="TransactionLines">
                                <RowTemplate>                                    
                                    <px:PXSegmentMask runat="server" ID="edInventoryIDTL" DataField="InventoryID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Option" Width="90px" MatrixMode="true" CommitChanges="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="BranchID" Width="108px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="TaskID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="InventoryID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="CostCodeID" Width="108px" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="Description" Width="180px" />
                                    <px:PXGridColumn DataField="ResourceID" Width="180px" />
                                    <px:PXGridColumn DataField="VendorID" Width="180px" />
                                    <px:PXGridColumn DataField="Date" Width="180px" />
                                    <px:PXGridColumn DataField="BillableQty" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryBillableAmount" TextAlign="Right" Width="90px" />
                                   
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                     <px:PXGridColumn DataField="UOM" Width="63px" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" Width="99px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryAmount" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryPrepaidAmount" TextAlign="Right" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="CuryMaxAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryAvailableAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryLineTotal" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryOverflowAmount" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="RetainagePct" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="CuryRetainage" TextAlign="Right" Width="90px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="100px" CommitChanges="true"/>
                                    <px:PXGridColumn DataField="AccountID" Width="100px" />
                                    <px:PXGridColumn DataField="SubID" Width="100px" />
                                    <px:PXGridColumn DataField="DefCode" Width="100px" />
                                    <px:PXGridColumn DataField="SortOrder" Width="63px" />
                                    <px:PXGridColumn DataField="LineNbr" Width="63px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton Text="Upload Unbilled Transactions" Tooltip="Upload Unbilled Transactions">
                                    <AutoCallBack Command="UploadUnbilled" Target="ds">
                                        <Behavior CommitChanges="True" />
                                    </AutoCallBack>
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="View Transaction Details" PopupPanel="DetailsPanel" />
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="True" AllowDragRows="true" />
                        <CallbackCommands PasteCommand="TransactPasteLine">
                            <%--<Save PostData="Container" />--%>
                        </CallbackCommands>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details">
                <Template>
                    <px:PXGrid ID="TaxDetailsGrid" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="500">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSelector CommitChanges="True" ID="edTaxID" runat="server" DataField="TaxID" />
                                    <px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit ID="edTaxableAmt" runat="server" DataField="TaxableAmt" />
                                    <px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="TaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" Width="100px" />
                                    <px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryExemptedAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CuryRetainedTaxableAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CuryRetainedTaxAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="Tax__TaxType" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__PendingTax" Type="CheckBox" TextAlign="Center" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__ReverseTax" Type="CheckBox" TextAlign="Center" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__ExemptTax" Type="CheckBox" TextAlign="Center" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__StatisticalTax" Type="CheckBox" TextAlign="Center" Width="60px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Invoice Settings" ControlSize="XM" LabelsWidth="SM" />
                     <px:PXDropDown ID="edARInvoiceDocType" runat="server" DataField="ARInvoiceDocType" DisplayMode="Text" Enabled="False" />
                     <px:PXSelector ID="edARInvoiceRefNbr" runat="server" DataField="ARInvoiceRefNbr" Enabled="False" AllowEdit="True"/>
                    <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="true"/>
                    <px:PXSelector ID="edTaxZone" runat="server" DataField="TaxZoneID" CommitChanges="true"/>
                     <px:PXDropDown ID="edAvalaraCustomerUsageType" runat="server" DataField="AvalaraCustomerUsageType" CommitChanges="true"/>
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" CommitChanges="true"/>
                    <px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
                    <px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />

                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />

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
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Address Details">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXFormView ID="Billing_Contact" runat="server" Caption="BILL-TO CONTACT" DataMember="Billing_Contact" RenderStyle="Fieldset">
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
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="Shipping_Contact" runat="server" Caption="SHIP-TO CONTACT" DataMember="Shipping_Contact" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommandSourceID="ds" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Shipping_Address" runat="server" Caption="SHIP-TO ADDRESS" DataMember="Shipping_Address" RenderStyle="Fieldset">
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
                </Template>
            </px:PXTabItem>

        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
