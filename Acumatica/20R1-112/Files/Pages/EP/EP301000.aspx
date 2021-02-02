<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP301000.aspx.cs"
    Inherits="Page_EP301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource  EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.ExpenseClaimEntry" PrimaryView="ExpenseClaim">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Release" Visible="false" />
            <px:PXDSCallbackCommand Name="ChangeOk" Visible="false" />
            <px:PXDSCallbackCommand Name="ChangeCancel" Visible="false" />
            <px:PXDSCallbackCommand Name="SaveTaxZone" Visible="false" />
            <px:PXDSCallbackCommand Name="CommitTaxes" Visible="false" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" RepaintControls="All" CommitChanges="True" />
            <px:PXDSCallbackCommand Visible="False" Name="createNew" RepaintControls="All" CommitChanges="False" />
            <px:PXDSCallbackCommand Visible="False" Name="SubmitReceipt" />
            <px:PXDSCallbackCommand Visible="False" Name="CancelSubmitReceipt" />
            <px:PXDSCallbackCommand Visible="False" Name="EditDetail"  RepaintControls="All" CommitChanges="True" HideText="True" PopupCommand="Cancel"/>
            <px:PXDSCallbackCommand Visible="False" Name="ShowSubmitReceipt" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewUnsubmitReceipt" />
            <px:PXDSCallbackCommand Visible="False" Name="Edit" />
            <px:PXDSCallbackCommand Visible="false" Name="Submit"/>
            <px:PXDSCallbackCommand Visible="False" Name="Approve" />
            <px:PXDSCallbackCommand Visible="False" Name="Reject" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewTaxes" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewProject" Visible="False"/>
			      <px:PXDSCallbackCommand DependOnGrid="apDocsGrig" Name="ViewInvoice" Visible="False"/>
            <px:PXDSCallbackCommand Name="Action@Submit" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ExpenseClaim" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
        NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edRefNbr" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXCheckBox ID="edHold" runat="server" DataField="Hold" Visible="False"/>
            <px:PXCheckBox ID="edApproved" runat="server" DataField="Approved" Visible="False"/>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" DisplayMode="Value" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" CommitChanges="True"/>
            <px:PXDateTimeEdit CommitChanges="True" ID="edApproveDate" runat="server" DataField="ApproveDate" Enabled="False" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit CommitChanges="True" ID="edDocDesc" runat="server" DataField="DocDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="PXCurrencyRate1" runat="server" DataSourceID="ds" RateTypeView="_EPExpenseClaim_CurrencyInfo_" DataMember="_Currency_" />
            <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" Enabled="False" DataSourceID="ds" />
            <px:PXPanel runat="server" RenderStyle="Simple" ID="CusomerPanel">
                <px:PXLayoutRule ID="PXLayoutRuleCustomer" runat="server" LabelsWidth="S" ControlSize="XM" />
                <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
                <px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" />
            </px:PXPanel>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
            <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
         </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="PanelSubmitReceipts" runat="server" Height="396px" Width="910px" Caption="Add Receipts" CaptionVisible="True" Key="ReceiptsForSubmit" AutoReload="true" CancelButtonID="PXButtonCancel" AutoRepaint="True" CallBackMode-CommitChanges="True">
        <px:PXGrid ID="gridReceiptsForSubmit" runat="server" Height="240px" Width="100%" DataSourceID="ds" SkinID="Inquire" NoteIndicator="false" FilesIndicator="false" SyncPosition="True" >
            <AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="ReceiptsForSubmit"  DataKeyNames="ClaimDetailID">
                    <Columns>
                        <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" CommitChanges="True"/>
						<px:PXGridColumn DataField="ClaimDetailCD" />
                        <px:PXGridColumn DataField="ExpenseDate" />
                        <px:PXGridColumn DataField="ExpenseRefNbr" />
                        <px:PXGridColumn DataField="EmployeeID" />
                        <px:PXGridColumn DataField="BranchID" />
                        <px:PXGridColumn DataField="TranDesc"  LinkCommand="ViewUnsubmitReceipt" />
                        <px:PXGridColumn DataField="CuryTranAmtWithTaxes" TextAlign="Right" />
                        <px:PXGridColumn DataField="CuryID" TextAlign="Right"/>
                        <px:PXGridColumn DataField="Status"/>
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
        </px:PXGrid>
        <px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonAdd" runat="server" Text="Add" CommandName="SubmitReceipt" CommandSourceID="ds" />
            <px:PXButton ID="PXButtonAddClose" runat="server" Text="Add & Close" CommandSourceID="ds" DialogResult="OK" />
            <px:PXButton ID="PXButtonClose" runat="server" DialogResult="Cancel" Text="Close" CommandName="CancelSubmitReceipt" CommandSourceID="ds" />      
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="CustomerUpdateAnswerPanel" runat="server" CaptionVisible="False" Key="CustomerUpdateAsk" CancelButtonID="ChangeCancel" AcceptButtonID="ChangeOk" >
        <px:PXFormView runat="server" DataSourceID="ds" DataMember="CustomerUpdateAsk" SkinID="Transparent">
            <Template>
                <px:PXLabel ID="PXLabel1" runat="server" Encode="false" Text="<B>Do you want to update customer in expense claim detail lines<br>that are not linked to project or contract?</B>" />
                <px:PXPanel runat="server" RenderStyle="Simple" >
                <px:PXGroupBox ID="CustomerUpdateAnswer" runat="server" DataField="CustomerUpdateAnswer" RenderStyle="Simple" CommitChanges="True" style="margin-top: 10px" >
				    <Template>
				        <px:PXLayoutRule ID="PXLayoutRule10" runat="server" />
					    <px:PXRadioButton ID="SelectedCusnomerLine" runat="server" GroupName="CustomerUpdateAnswer" Value="S" />
					    <px:PXRadioButton ID="AllLine" runat="server" GroupName="CustomerUpdateAnswer" Value="A" />
					    <px:PXRadioButton ID="Nothing" runat="server" GroupName="CustomerUpdateAnswer" Value="N" />
				    </Template>
			    </px:PXGroupBox>
                </px:PXPanel>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="CustomerUpdateAnswerButton" runat="server" SkinID="Buttons">
            <px:PXButton ID="ChangeOk" runat="server" DialogResult="Yes" Text="Change" CommandName="ChangeOk" CommandSourceID="ds" />
            <px:PXButton ID="ChangeCancel" runat="server" DialogResult="No" Text="Cancel" CommandName="ChangeCancel" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PXTaxPanel" runat="server" CaptionVisible="True" Key="Tax_Rows" AcceptButtonID="Ok"  AutoRepaint="true" AutoReload="true"
         Width="764px" Height="360px" Caption="Document Taxes" >
      <px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
		  <px:PXFormView runat="server" DataMember="ExpenseClaimDetailsCurrent" SkinID="Transparent" ID="frmDiscrepancy">
                <Template>
                        <px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
					    <px:PXNumberEdit ID="edTaxDiff" runat="server" DataField="CuryTaxRoundDiff" Enabled="false" />
                    </Template>
            </px:PXFormView>
        <px:PXGrid ID="PXGrid1" runat="server" Width="100%"
			DataSourceID="ds" SkinID="Details" AdjustPageSize="Auto"
            NoteIndicator="false" FilesIndicator="false" >
            <AutoSize Enabled="true" />
            
            <Levels>
                 
                <px:PXGridLevel DataMember="Tax_Rows">
                   	<Columns>
						<px:PXGridColumn DataField="TaxID" />
						<px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
						<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" CommitChanges="true" />
						<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right"  CommitChanges="true" />
						<px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" />
						<px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" CommitChanges="true" />
						<px:PXGridColumn DataField="Tax__TaxType"/>
						<px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox"/>
						<px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox"/>
						<px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox"/>
						<px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" Type="CheckBox"/>
					</Columns>
                </px:PXGridLevel>
            </Levels>
			<AutoSize Enabled="true" />
         
        </px:PXGrid>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton1" runat="server" DialogResult="Yes" Text="Ok" CommandName="CommitTaxes" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
     <px:PXSmartPanel ID="TaxZoneUpdateAsk" runat="server" CaptionVisible="True" Key="TaxZoneUpdateAskView" CancelButtonID="CancelTaxZone" 
         AcceptButtonID="SaveTaxZone" >
          <px:PXFormView runat="server" DataMember="TaxZoneUpdateAskView" SkinID="Transparent" ID="PXFormView1">
                <Template>
                        <px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
					    <px:PXLabel ID="Label" runat="server" Text="Do you want to use the specified tax zone for expense receipts by default?" />
                    
                    </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="SaveTaxZone" runat="server" DialogResult="Yes" Text="Yes" CommandName="SaveTaxZone" CommandSourceID="ds" />
            <px:PXButton ID="CancelTaxZone" runat="server" DialogResult="No" Text="No"  />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%" DataMember="ExpenseClaimCurrent" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Expense Claim Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="100%" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="Top" BorderWidth="0px" SkinID="DetailsInTab" TabIndex="1100" SyncPosition="True" >
                        <CallbackCommands>
                            <Save PostData="Container" />
                            <Refresh CommitChangesIDs="form" RepaintControls="All" />
                        </CallbackCommands>
                        <Levels>
                            <px:PXGridLevel DataMember="ExpenseClaimDetails" DataKeyNames="ClaimDetailID">
                                <Columns>
									<px:PXGridColumn DataField="ClaimDetailCD"/>
                                    <px:PXGridColumn DataField="ExpenseDate" TextCase="Upper" />
                                    <px:PXGridColumn DataField="ExpenseRefNbr" />
                                    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TranDesc" LinkCommand="EditDetail" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" AutoCallBack="True"  />
                                    <px:PXGridColumn DataField="CuryExtCost" TextAlign="Right" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="CuryTaxTotal" TextAlign="Right" LinkCommand="ViewTaxes" />
                                    <px:PXGridColumn DataField="CuryEmployeePart" TextAlign="Right" CommitChanges="True"/>
									<px:PXGridColumn DataField="CuryTipAmt" TextAlign="Right" CommitChanges="True" />
									<px:PXGridColumn DataField="CuryNetAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTranAmtWithTaxes" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryID" />
                                    <px:PXGridColumn DataField="ClaimCuryTranAmtWithTaxes"/>
                                    <px:PXGridColumn DataField="Status" />
                                    <px:PXGridColumn DataField="CustomerID" Width="100px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CustomerLocationID" Width="70px" />
                                    <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" TextCase="Upper" />
                                    <px:PXGridColumn DataField="ContractID" AutoCallBack="True" LinkCommand="ViewProject"/>
                                    <px:PXGridColumn DataField="TaskID" Label="Task" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="CostCodeID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PaidWith" CommitChanges="True" />
                                    <px:PXGridColumn DataField="CorpCardID" DisplayMode="Hint" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="ExpenseAccountID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ExpenseSubID" />
                                    <px:PXGridColumn DataField="SalesAccountID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SalesSubID" />
                                    <px:PXGridColumn DataField="TaxCategoryID" />
									<px:PXGridColumn DataField="TaxZoneID" />
									<px:PXGridColumn DataField="TaxCalcMode" />
                                    <px:PXGridColumn DataField="BranchID" />
                                    <px:PXGridColumn DataField="ARRefNbr"/>
									<px:PXGridColumn DataField="APRefNbr"/>
									<px:PXGridColumn DataField="EmployeeID" Visible="False"/>
									<px:PXGridColumn DataField="CreatedFromClaim" Visible="False"/>
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
									<px:PXTextEdit ID="edClaimDetailCD" runat="server" DataField="ClaimDetailCD" />
                                    <pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" DataSourceID="ds" RateTypeView="_EPExpenseClaimDetails_CurrencyInfo_"
                                                        DataMember="_Currency_" CommitChanges="True"></pxa:PXCurrencyRate>
                                    <px:PXDateTimeEdit ID="edExpenseDate" runat="server" DataField="ExpenseDate" />
                                    <px:PXTextEdit ID="edExpenseRefNbr" runat="server" DataField="ExpenseRefNbr" />
                                    <px:PXSelector ID="edCorpCardID" runat="server" DataField="CorpCardID" CommitChanges="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" Size="XM" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" CommitChanges="True" />
                                    <px:PXNumberEdit ID="edCuryTotalAmount" runat="server" DataField="CuryExtCost" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTipAmt" runat="server" DataField="CuryTipAmt" CommitChanges="True" />
									<px:PXNumberEdit ID="edNetAmount" runat="server" DataField="CuryNetAmount" />
                                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmtWithTaxes" />
                                    <px:PXNumberEdit ID="edTaxAmount" runat="server" DataField="CuryTaxTotal" />
                                    <px:PXNumberEdit ID="edCuryEmployeePart" runat="server" DataField="CuryEmployeePart" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" Size="XM" />
                                    <px:PXSegmentMask CommitChanges="True" Size="XM" ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" />
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                                    <px:PXCheckBox ID="chkBillable" runat="server" DataField="Billable" />
                                    <px:PXSegmentMask ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" />
                                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edSalesAccountID" runat="server" DataField="SalesAccountID" />
                                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
                                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
                                    <px:PXSegmentMask CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID" Height="18px" AllowEdit="True" AutoRefresh="True">
                                        <GridProperties FastFilterFields="Description" />
                                    </px:PXSegmentMask>
									<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" AutoRefresh="True" />
									<px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" AutoRefresh="True"/>
                                    <px:PXSegmentMask ID="edTaskID" runat="server" AutoRefresh="True" DataField="TaskID" />
                                    <px:PXSegmentMask ID="edCostCode" runat="server" DataField="CostCodeID" AutoRefresh="True" AllowAddNew="true" />
                                    <px:PXTextEdit ID="edCustomer_acctName" runat="server" DataField="CustomerID_Customer_acctName" Enabled="False" />
                                    <px:PXSelector ID="edArInvoiceLink" runat="server" DataField="ARRefNbr" AllowEdit="True" Size="XM"/>
									<px:PXSelector ID="edApInvoiceLink" runat="server" DataField="APRefNbr" AllowEdit="True" Size="XM"/>
                                    <px:PXTextEdit ID="edProjectDescription" runat="server" DataField="ProjectDescription" />
                                    <px:PXTextEdit ID="edProjectTaskDescription" runat="server" DataField="ProjectTaskDescription"/>
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="160" />
                        <Mode InitNewRow="True" AllowFormEdit="False" AllowUpload="True" />
                        <LevelStyles>
                            <RowForm Height="330px" Width="950px" />
                        </LevelStyles>
                        <ActionBar>
			                <CustomItems>
				                <px:PXToolBarButton Key="cmdViewReceipt" DisplayStyle="Image" >
					                <AutoCallBack Target="ds" Command="EditDetail" />
                					<PopupCommand Target="form" Command="Refresh" />
					                <Images Normal="main@DataEntry" />
                                    <ActionBar GroupIndex="0" Order="2" />
				                </px:PXToolBarButton>
				                <px:PXToolBarButton Key="cmdcreateNew" DisplayStyle="Text" >
					                <AutoCallBack Target="ds" Command="createNew" />
                					<PopupCommand Target="form" Command="Refresh" />
				                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdSubmitReceipt" DisplayStyle="Text" >
					                <AutoCallBack Target="ds" Command="ShowSubmitReceipt"  />
				                </px:PXToolBarButton>
			                </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details">
                <Template>
					<px:PXFormView runat="server" DataMember="ExpenseClaimCurrent" SkinID="Transparent">
						<Template>
								<px:PXLayoutRule ID="PXLayoutRule232" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"  />
								<px:PXNumberEdit ID="edTaxDiff" runat="server" DataField="CuryTaxRoundDiff" Enabled="false" />
						</Template>
					</px:PXFormView>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
                        BorderWidth="0px" SkinID="Details">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                                <Save Enabled="False" />
                                <EditRecord Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes" DataKeyNames="RefNbr,ClaimDetailID,TaxID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" Visible="False"/>

                                    <px:PXGridColumn DataField="TaxID" />
                                    <px:PXGridColumn DataField="TaxRate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" />
                                </Columns>
                             <Mode AllowAddNew="false" AllowDelete="false" />
                            </px:PXGridLevel>
                        </Levels>
                            <ActionBar>
                                  <Actions>
                                      <AddNew ToolBarVisible="False" />
                                      <Delete ToolBarVisible="False" />
                                  </Actions>
                            </ActionBar>
                        <Mode AllowFormEdit="False" AllowUpload="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" ControlSize="XM" GroupCaption="Default Tax Info" LabelsWidth="SM" StartGroup="True" />
                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
                    <px:PXDropDown ID="edTaxCalcMode" runat="server" DataField="TaxCalcMode" CommitChanges="true"/>

                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to AP" StartGroup="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                    <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges="True" />
                   
                    <px:PXGrid ID="apDocsGrig" runat="server"  Width="650px" Height="100px" TabIndex="11900" DataSourceID="ds"
                       AutoAdjustColumns="true" FilesIndicator="false" NoteIndicator="false" SkinID="ShortList">
                       <Levels>
						   <px:PXGridLevel DataMember="APDocuments">
							   <Columns>
								   <px:PXGridColumn DataField="DocType" />
								   <px:PXGridColumn DataField="RefNbr" AutoCallBack="True" LinkCommand="ViewInvoice" />
								   <px:PXGridColumn DataField="CuryOrigDocAmt" />
								   <px:PXGridColumn DataField="TaxZoneID" />
								   <px:PXGridColumn DataField="TaxCalcMode" />
								   <px:PXGridColumn DataField="Status" />
							   </Columns>
						   </px:PXGridLevel>
					   </Levels>
                      <ActionBar ActionsVisible="false"></ActionBar>
                  </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
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
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" />
    </px:PXTab>
    <!--#include file="~\Pages\Includes\CRApprovalReasonPanel.inc"-->
</asp:Content>
