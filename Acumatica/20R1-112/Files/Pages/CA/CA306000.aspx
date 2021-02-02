<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA306000.aspx.cs" Inherits="Page_BankStatementProto" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TranFilter" TypeName="PX.Objects.CA.CABankTransactionsMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="matchSettingsPanel" PopupPanel="pnlMatchSettings" Visible="True" />
            <px:PXDSCallbackCommand Name="ProcessMatched" CommitChanges="true" Visible="True" />
            <px:PXDSCallbackCommand Name="UploadFile" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="RefreshAfterRuleCreate" CommitChanges="true" Visible="false"/>
            <px:PXDSCallbackCommand Name="ClearMatch" Visible="false" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="ClearAllMatches" Visible="false" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="Hide" Visible="false" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="CreateRule" Visible="false" PopupPanel="pnlCreateRule" CommitChanges="true" PopupCommand="RefreshAfterRuleCreate"/>
            <px:PXDSCallbackCommand Name="UnapplyRule" Visible="false" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewPayment" Visible="false" DependOnGrid="PXGrid1" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="false" DependOnGrid="gridDetailMatches4" />
            <px:PXDSCallbackCommand Name="ViewDocumentToApply" Visible="false"  />
            <px:PXDSCallbackCommand Visible="False" CommitChanges="True" Name="LoadInvoices" />
            <px:PXDSCallbackCommand Visible="False" Name="ViewExpenseReceipt"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="PXSplitContainer" Height="100%" PositionInPercent="true"
        SplitterPosition="50" Panel2MinSize="400" Size="3" SkinID="Transparent" Style="border-right: 0px;">
        <AutoSize Enabled="true" Container="Window" />
        <ContentLayout AutoSizeControls="True" Orientation="Horizontal" />
        <Template1>
            <px:PXUploadDialog ID="pnlNewRev" Key="NewRevisionPanel" runat="server" Height="120px" Style="position: static" Width="560px" Caption="Statement File Upload" AutoSaveFile="false" RenderCheckIn="false" SessionKey="ImportStatementProtoFile" />
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100; border-right: 1px solid #BBBBBB;"
                Width="100%" DataMember="TranFilter" TabIndex="1200">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask runat="server" DataField="CashAccountID" ID="edCashAccountID" CommitChanges="true" Required="true" AllowAddNew="True" AllowEdit="True"/>
                    <px:PXCheckBox runat="server" DataField="IsCorpCardCashAccount" ID="edIsCorpCardCashAccount" />
                </Template>
            </px:PXFormView>
            <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Style="z-index: 100; border-right: 1px solid #BBBBBB;"
                Height="100%" Width="100%" SyncPosition="True" SkinID="Details" TabIndex="300" AllowFilter="true" CaptionVisible="true"
                AutoAdjustColumns="True" StatusField="MatchStatsInfo" FilesIndicator="False" FeedbackMode="ForceDataEntry">
                <GridStyles>
                    <Caption Height="23px" />
                </GridStyles>
                <AutoCallBack Target="PXGrid1" Command="Refresh" ActiveBehavior="true">
                    <Behavior RepaintControls="None" RepaintControlsIDs="form,grid1,gridAdjustments,gridCASplits,gridDetailMatches4,gridExpenseReceiptDetailMatches,frmCreateDocumentInv,frmCreateDocument,tab2" />
                </AutoCallBack>
                <ActionBar ActionsVisible="true" DefaultAction="ViewDetailsDoc" PagerVisible="False" ActionsText="False">
                    <Actions>
                        <AdjustColumns Enabled="true" MenuVisible="true" GroupIndex="1" Order="5" ToolBarVisible="Top" />
                        <Refresh Enabled="true" MenuVisible="true" GroupIndex="1" Order="1" ToolBarVisible="Top" />
                        <AddNew Enabled="false" MenuVisible="false" />
                        <Delete Enabled="false" MenuVisible="false" />
                        <ExportExcel Enabled="false" MenuVisible="false" />
                    </Actions>
                    <CustomItems>
                        <px:PXToolBarButton Text="Clear Match" Key="cmdLS" CommandName="ClearMatch" CommandSourceID="ds" DependOnGrid="grid1">
                            <ActionBar GroupIndex="1" Order="2" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Clear Match" Key="cmdLS" CommandName="ClearAllMatches" CommandSourceID="ds" DependOnGrid="grid1">
                            <ActionBar GroupIndex="1" Order="3" />
                        </px:PXToolBarButton>
                        <px:PXToolBarButton Text="Hide Transaction" Key="cmdLS" CommandName="Hide" CommandSourceID="ds" DependOnGrid="grid1">
                            <ActionBar GroupIndex="1" Order="4" />
                        </px:PXToolBarButton>
                    </CustomItems>
                </ActionBar>
                <Levels>
                    <px:PXGridLevel DataKeyNames="CashAccountID,TranID" DataMember="Details">
                        <RowTemplate>
                        </RowTemplate>
                        <Columns>
                            <px:PXGridColumn DataField="Status" Type="DropDownList">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="DocumentMatched" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="RuleApplied" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ApplyRuleEnabled" TextAlign="Center" Type="CheckBox">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ExtTranID">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="ExtRefNbr">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TranDate">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right"> 
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="CardNumber"/>
                            <px:PXGridColumn DataField="TranDesc">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TranCode">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="TranEntryDate">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PayeeName">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="EntryTypeID1">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="InvoiceInfo1">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PaymentMethodID1">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PayeeBAccountID1">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="AcctName">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="OrigModule1">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="PayeeLocationID1">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" />
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXTab ID="tab2" runat="server" Width="100%" Style="background-color: #ffffff;" 
                              SelectedIndexExpr='<%# 
                                        ((PXCheckBox)Container.FindControl("form").FindControl("edIsCorpCardCashAccount")).Checked
                                            ? (
                                                    ((PXGrid)Container.FindControl("grid1")).DataValues["Status"] != null ? (
                                                    (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "M" ? 0 :
                                                    (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "I" ? 1 :
                                                    (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "R" ? 2 :
                                                    (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "C" ? 3 : 0):
                                                    ((PXGrid)Container.FindControl("tab2").FindControl("PXGrid1")).Rows.Count > 0 ? 0 :
                                                    ((PXGrid)Container.FindControl("tab2").FindControl("gridDetailMatches4")).Rows.Count > 0 ? 1 :
                                                    ((PXGrid)Container.FindControl("tab2").FindControl("gridExpenseReceiptDetailMatches")).Rows.Count > 0 ? 2 : 3
                                                )
                                            :(
                                                ((PXGrid)Container.FindControl("grid1")).DataValues["Status"] != null ? (
                                                (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "M" ? 0 :
                                                (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "I" ? 1 :
                                                (string)((PXGrid)Container.FindControl("grid1")).DataValues["Status"] == "C" ? 3 : 0):
                                                ((PXGrid)Container.FindControl("tab2").FindControl("PXGrid1")).Rows.Count > 0 ? 0 :
                                                ((PXGrid)Container.FindControl("tab2").FindControl("gridDetailMatches4")).Rows.Count > 0 ? 1 : 3
                                            )
                                    %>'
                      >
                <AutoSize Enabled="true" />
                <Items>
                    <px:PXTabItem Text="Match to Payments" RepaintOnDemand="False" BindingContext="form">
                        <Template>
                            <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Style="z-index: 100"
                                Height="100%" Width="100%" SyncPosition="True" SkinID="Details" TabIndex="300" 
                                FilesIndicator="False" NoteIndicator="False" FeedbackMode="ForceDataEntry">
                                <Mode InitNewRow="True" />
                                <CallbackCommands>
                                    <Save RepaintControls="None" RepaintControlsIDs="grid1,frmCreateDocument" />
                                </CallbackCommands>
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="grid1" />
                                </Parameters>
                                <ActionBar Position="TopAndBottom" DefaultAction="ViewDetailsDoc" PagerVisible="False">
                                    <Actions>
                                        <AddNew Enabled="False" MenuVisible="False" />
                                        <Delete Enabled="False" MenuVisible="False" />
                                        <ExportExcel Enabled="False" MenuVisible="False" />
                                    </Actions>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="DetailMatchesCA" DataKeyNames="TranID">
                                        <RowTemplate>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="IsMatched" TextAlign="Center" Type="CheckBox" CommitChanges="True" AllowSort="False" />
                                            <px:PXGridColumn DataField="MatchRelevancePercent" />
                                            <px:PXGridColumn DataField="OrigRefNbr" LinkCommand="ViewPayment" />
                                            <px:PXGridColumn DataField="TranDate" />
                                            <px:PXGridColumn DataField="ExtRefNbr" />
                                            <px:PXGridColumn DataField="OrigModule" />
                                            <px:PXGridColumn DataField="OrigTranType" Type="DropDownList" MatrixMode="true" />
                                            <px:PXGridColumn DataField="TranDesc" />
                                            <px:PXGridColumn DataField="FinPeriodID" />
                                            <px:PXGridColumn DataField="CuryTranAbsAmt" TextAlign="Right" />
                                            <px:PXGridColumn DataField="ReferenceID" />
                                            <px:PXGridColumn DataField="ReferenceID_description"  />
                                        </Columns>

                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Match to Invoices" RepaintOnDemand="False" BindingContext="form">
                        <Template>
                            <px:PXFormView ID="frmCreateDocumentInv" runat="server" DataSourceID="ds" DataMember="DetailsForInvoiceApplication" Style="z-index: 100"
                                Width="100%" Caption="Create Document" CaptionVisible="false" FilesIndicator="false" 
                                NoteIndicator="false" SkinID="Preview" >
                                <CallbackCommands>
                                    <Save RepaintControls="None" RepaintControlsIDs="grid1,gridDetailMatches4,frmCreateDocument" />
                                </CallbackCommands>
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule26" runat="server" LabelsWidth="sm" ControlSize="m" StartRow="true" />
                                    <px:PXCheckBox ID="edMultipleMatching" runat="server" DataField="MultipleMatching" CommitChanges="true" />
                                    <px:PXSelector ID="edPayeeBAccountID" runat="server" DataField="PayeeBAccountIDCopy"
                                        Size="M" CommitChanges="true" AutoRefresh="True" Enabled="false" />
                                    <px:PXSegmentMask ID="edPayeeLocationID" runat="server" DataField="PayeeLocationIDCopy"
                                        Size="M" CommitChanges="true" AutoRefresh="True" />
                                    <px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodIDCopy"
                                        Size="M" CommitChanges="true" AutoRefresh="true" />
                                    <px:PXSelector ID="edPMInstanceID" runat="server" DataField="PMInstanceIDCopy" CommitChanges="true"
                                        Size="SM" />
                                    <px:PXLayoutRule ID="PXLayoutRule29" runat="server" LabelsWidth="sm" StartColumn="true" />
                                    <px:PXLabel runat="server" Text=""/>
                                    <px:PXNumberEdit ID="edCuryTotalAmtCopy" runat="server" DataField="CuryTotalAmtCopy" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryApplAmtMatch" runat="server" DataField="CuryApplAmtMatch" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryUnappliedBalMatch" runat="server" DataField="CuryUnappliedBalMatch" Enabled="false" />
                                </Template>
                            </px:PXFormView>
                            <px:PXGrid ID="gridDetailMatches4" runat="server" Width="100%" DataSourceID="ds"
                                SkinID="Details" Caption="Matching Invoices" FilesIndicator="false" FeedbackMode="ForceDataEntry">
                                <CallbackCommands>
                                    <Save RepaintControls="None" RepaintControlsIDs="grid1,frmCreateDocumentInv,frmCreateDocument" />
                                </CallbackCommands>
                                <Mode AllowAddNew="false" AllowDelete="false" AllowRowSizing="true" />
                                <AutoSize Enabled="true" />
                                <Layout WrapText="True" />
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="grid1" />
                                </Parameters>
                                <Levels>
                                    <px:PXGridLevel DataMember="detailMatchingInvoices">
                                        <RowTemplate>
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="IsMatched" Label="IsMatched" TextAlign="Center" Type="CheckBox"
                                                Width="72px" AutoCallBack="True" AllowSort="False" />
                                            <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="MatchRelevancePercent"
                                                Label="Match Relevance" TextAlign="Right" Width="90px" />
                                            <px:PXGridColumn DataField="BranchID"  />
                                            <px:PXGridColumn DataField="OrigModule" Label="Module" />
                                            <px:PXGridColumn DataField="OrigTranType" Label="Tran. Type" RenderEditorText="True" Type="DropDownList" MatrixMode="true" />
                                            <px:PXGridColumn DataField="OrigRefNbr" Label="Tran. Type" RenderEditorText="True" LinkCommand="ViewInvoice" />
                                            <px:PXGridColumn DataField="ExtRefNbr" Label="Document Ref." />
                                            <px:PXGridColumn DataField="TranDate" Label="Doc. Date" />
                                            <px:PXGridColumn AllowNull="False" DataField="CuryTranAmt" Label="Amount" TextAlign="Right"/>
                                            <px:PXGridColumn DataField="CuryDiscAmt" Label="Cash Discount" TextAlign="Right"/>
                                            <px:PXGridColumn DataField="DiscDate" Label="Discount Date" />
                                            <px:PXGridColumn DataField="ReferenceID" Label="Business Account" />
                                            <px:PXGridColumn DataField="ReferenceName" Label="Account Name" />
                                            <px:PXGridColumn DataField="TranDesc" Label="CATran-Description" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar Position="TopAndBottom" DefaultAction="ViewDoc" PagerVisible="False">
                                    <Actions>
                                        <AddNew Enabled="False" MenuVisible="False" />
                                        <Delete Enabled="False" MenuVisible="False" />
                                        <ExportExcel Enabled="False" MenuVisible="False" />
                                    </Actions>
                                    <CustomItems>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Match to Expense Receipts" RepaintOnDemand="False" BindingContext="form" VisibleExp="DataControls[&quot;edIsCorpCardCashAccount&quot;].Value == true">
                        <Template>
                          <px:PXGrid ID="gridExpenseReceiptDetailMatches" runat="server" SkinID="Details" Width="100%" FeedbackMode="ForceDataEntry" Caption="Matching Expense Receipts" DataSourceID="ds" FilesIndicator="false">
                            <AutoSize Enabled="true" />
                            <ActionBar DefaultAction="ViewDoc" Position="TopAndBottom" PagerVisible="False">
                              <Actions>
                                <AddNew Enabled="False" MenuVisible="False" />
                                <Delete Enabled="False" MenuVisible="False" />
                                <ExportExcel Enabled="False" MenuVisible="False" /></Actions>
                              <CustomItems /></ActionBar>
                            <CallbackCommands>
                              <Save RepaintControls="None" RepaintControlsIDs="grid1" /></CallbackCommands>
                            <Mode AllowAddNew="false" AllowDelete="false" AllowRowSizing="true" />
                            <Layout WrapText="True" />
                            <Levels>
                              <px:PXGridLevel DataMember="ExpenseReceiptDetailMatches">
                                <RowTemplate />
                                <Columns>
                                  <px:PXGridColumn Type="CheckBox" TextAlign="Center" DataField="IsMatched" Width="72px" AutoCallBack="True" CommitChanges="True" AllowSort="False" />
                                  <px:PXGridColumn TextAlign="Right" DataField="MatchRelevancePercent" Width="90px" />
                                  <px:PXGridColumn DataField="RefNbr" Width="90px" LinkCommand="ViewExpenseReceipt"/>
                                  <px:PXGridColumn DataField="TranDesc" Width="180px" />
                                  <px:PXGridColumn DataField="DocDate" Width="80px" />
                                  <px:PXGridColumn TextAlign="Right" DataField="CuryDocAmt" Width="78px" />
                                  <px:PXGridColumn TextAlign="Right" DataField="ClaimCuryID" Width="65px"/>
                                  <px:PXGridColumn TextAlign="Right" DataField="CuryDocAmtDiff" Width="78px" />
                                  <px:PXGridColumn DataField="CardNumber" Width="80px" />
                                  <px:PXGridColumn DataField="ReferenceID" Width="100px" />
                                  <px:PXGridColumn DataField="ReferenceName" Width="220px" />
                                  <px:PXGridColumn DataField="ExtRefNbr" Width="80px" />
                                  <px:PXGridColumn DataField="PaidWith" Width="250px"/>
                                </Columns></px:PXGridLevel></Levels>
                            <Parameters>
                              <px:PXSyncGridParam ControlID="grid1" />
                            </Parameters>
                          </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                    <px:PXTabItem Text="Create Payment" RepaintOnDemand="False" BindingContext="form">
                        <Template>
                            <px:PXFormView ID="frmCreateDocument" runat="server" DataSourceID="ds" DataMember="DetailsForPaymentCreation" Style="z-index: 100"
                                CaptionVisible="true" FilesIndicator="true" NoteIndicator="false" SkinID="Preview" Width="100%">
                                <CallbackCommands>
                                    <Save RepaintControls="None" RepaintControlsIDs="grid1,gridAdjustments,gridCASplits" />
                                </CallbackCommands>
                                <Template>
                                    <px:PXLayoutRule ID="PXLayoutRule26" runat="server" LabelsWidth="sm" ControlSize="m" />
                                    <px:PXCheckBox ID="edCreateDocument" runat="server" DataField="CreateDocument" CommitChanges="true" />
                                    <px:PXSelector runat="server" DataField="RuleID" ID="edRuleID" AllowEdit="true" Enabled="false" />

                                    <px:PXLayoutRule runat="server" Merge="true" />
                                    <px:PXLabel runat="server" Text="" Size="sm" />
                                    <px:PXButton ID="btnUnapplyRule" runat="server" CommandName="UnapplyRule" CommandSourceID="ds" SyncVisibility="true" CallbackUpdatable="true" DependOnGrid="grid1" StateColumn="RuleApplied" />
                                    <px:PXButton ID="btnCreateRule" runat="server" CommandName="CreateRule" CommandSourceID="ds" SyncVisibility="true" CallbackUpdatable="true" DependOnGrid="grid1" StateColumn="ApplyRuleEnabled"/>
                                    <px:PXLayoutRule runat="server" />

                                    <px:PXDropDown ID="edOrigModule" runat="server" DataField="OrigModule"
                                        CommitChanges="true" />
                                    <px:PXSelector ID="edEntryTypeID" runat="server" DataField="EntryTypeID" CommitChanges="true" AutoRefresh="true" />
                                    <px:PXSelector ID="edPayeeBAccountID" runat="server" DataField="PayeeBAccountID"
                                        CommitChanges="true" AutoRefresh="True" AllowEdit="true" />
                                    <px:PXSegmentMask ID="edPayeeLocationID" runat="server" DataField="PayeeLocationID"
                                        CommitChanges="true" AutoRefresh="True" />
                                    <px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID"
                                        CommitChanges="true" AutoRefresh="true" />
                                    <px:PXSelector ID="edPMInstanceID" runat="server" DataField="PMInstanceID" CommitChanges="true" />
                                    <px:PXTextEdit ID="edInvoiceInfo" runat="server" DataField="InvoiceInfo" />
                                    <px:PXTextEdit ID="edUserDesc" runat="server" DataField="UserDesc" />

                                    <px:PXLayoutRule ID="PXLayoutRule28" runat="server" LabelsWidth="sm" StartColumn="true" />
                                    <px:PXLabel runat="server" Text="" />
                                    <px:PXNumberEdit ID="edCuryTotalAmt" runat="server" DataField="CuryTotalAmt" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryApplAmt" runat="server" DataField="CuryApplAmt" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryUnappliedBal" runat="server" DataField="CuryUnappliedBal" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryWOAmt" runat="server" DataField="CuryWOAmt" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryApplAmtCA" runat="server" DataField="CuryApplAmtCA" Enabled="false" />
                                    <px:PXNumberEdit ID="edCuryUnappliedBalCA" runat="server" DataField="CuryUnappliedBalCA" Enabled="false" />

                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="sm" SuppressLabel="true" />
                                </Template>
                            </px:PXFormView>
                            <px:PXGrid ID="gridAdjustments" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds" SkinID="Details" Caption="AP/AR Adjustments" FilesIndicator="False" NoteIndicator="False"
                                SyncPosition="true">
                                <Mode InitNewRow="True" />
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="grid1" />
                                </Parameters>
                                <Levels>
                                    <px:PXGridLevel DataMember="Adjustments">
                                        <RowTemplate>
                                            <px:PXDropDown ID="edAdjdDocType" runat="server" DataField="AdjdDocType" CommitChanges="True" />
                                            <px:PXSelector ID="edAdjdRefNbr" runat="server" AutoRefresh="True" CommitChanges="True" DataField="AdjdRefNbr"/>                                            
											<px:PXSelector ID="edAdjdCustomerID" runat="server" AutoRefresh="True" DataField="ARInvoice__CustomerID"/>
                                            <px:PXNumberEdit ID="edCuryAdjgAmt" runat="server" CommitChanges="True" DataField="CuryAdjgAmt" />
                                            <px:PXNumberEdit ID="edCuryAdjgDiscAmt" runat="server" CommitChanges="True" DataField="CuryAdjgDiscAmt" />
                                            <px:PXNumberEdit ID="edCuryAdjgWhTaxAmt" runat="server" CommitChanges="True" DataField="CuryAdjgWhTaxAmt" />
                                            <px:PXDateTimeEdit ID="edAdjdDocDate" runat="server" DataField="AdjdDocDate" Enabled="False" />
                                            <px:PXNumberEdit ID="edAdjdCuryRate" runat="server" CommitChanges="True" DataField="AdjdCuryRate" />
                                            <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
                                            <px:PXNumberEdit ID="edCuryDiscBal" runat="server" DataField="CuryDiscBal" Enabled="False" />
                                            <px:PXNumberEdit ID="edCuryWhTaxBal" runat="server" DataField="CuryWhTaxBal" Enabled="False" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="AdjdBranchID" />
                                            <px:PXGridColumn DataField="AdjdDocType" Type="DropDownList" AutoCallBack="True" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="AdjdRefNbr" LinkCommand="ViewDocumentToApply"/>
											<px:PXGridColumn DataField="ARInvoice__CustomerID" TextAlign="Right" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgAmt" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgDiscAmt" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryDiscBal" TextAlign="Right" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="CuryAdjgWhTaxAmt" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryWhTaxBal" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="CuryAdjgWOAmt" AutoCallBack="True" TextAlign="Right" />
                                            <px:PXGridColumn AllowNull="False" DataField="WriteOffReasonCode" />

                                            <px:PXGridColumn DataField="AdjdDocDate" />
                                            <px:PXGridColumn DataField="AdjdFinPeriodID" />
                                            <px:PXGridColumn DataField="AdjdCuryID" />
                                            <px:PXGridColumn AutoCallBack="True" DataField="AdjdCuryRate" TextAlign="Right" />
                                        </Columns>
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
                            <px:PXGrid ID="gridCASplits" runat="server" AutoAdjustColumns="True" Caption="CA Splits" DataSourceID="ds" FilesIndicator="False" NoteIndicator="False" SkinID="Details" SyncPosition="true" Width="100%">
                                <Mode InitNewRow="True" AutoInsertField="CuryTranAmt" />
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="grid1" />
                                </Parameters>
                                <Levels>
                                    <px:PXGridLevel DataKeyNames="BankTranID,BankTranType,LineNbr" DataMember="TranSplit">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                                            <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" />
                                            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
                                            <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
                                            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                            <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                            <px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
                                            <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                                            <px:PXSelector ID="edCashAccountID" runat="server" DataField="CashAccountID" CommitChanges="True" />
                                            <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="True" />
                                            <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" CommitChanges="true" />
                                            <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True" />
                                            <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="BranchID" />
                                            <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="TranDesc" />
                                            <px:PXGridColumn DataField="Qty" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" />
                                            <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" CommitChanges="true" />
                                            <px:PXGridColumn DataField="CashAccountID" CommitChanges="True" />
                                            <px:PXGridColumn DataField="AccountID" CommitChanges="True" />
                                            <px:PXGridColumn DataField="AccountID_description" AllowUpdate="False" />
                                            <px:PXGridColumn DataField="SubID" />
                                            <px:PXGridColumn DataField="ProjectID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="TaskID" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="CostCodeID" />
                                            <px:PXGridColumn DataField="NonBillable" Label="Non Billable" Type="CheckBox" />
                                            <px:PXGridColumn DataField="TaxCategoryID" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template>
                    </px:PXTabItem>
                </Items>
            </px:PXTab>
        </Template2>
    </px:PXSplitContainer>
    <px:PXSmartPanel ID="pnlMatchSettings" runat="server" Style="z-index: 108;" Key="matchSettings"
        Caption="Transaction Match Settings" CaptionVisible="True" LoadOnDemand="true"
        ShowAfterLoad="true" AutoCallBack-Command="Refresh"
        AutoCallBack-Target="frmMatchSettings" DesignView="Content">
        <px:PXFormView ID="frmMatchSettings" runat="server" DataSourceID="ds"
            Style="z-index: 100" DataMember="cashAccount"
            Caption="Transaction Match Settings" CaptionVisible="False"
            SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule17" runat="server" LabelsWidth="L" ControlSize="XS" StartGroup="True" GroupCaption="Disbursement Matching" />
                <px:PXNumberEdit ID="edDisbursementTranDaysBefore" DataField="DisbursementTranDaysBefore" runat="server" />
                <px:PXNumberEdit ID="edDisbursementTranDaysAfter" DataField="DisbursementTranDaysAfter" runat="server" />
				<px:PXCheckBox ID="chkAllowMatchingCreditMemo" SuppressLabel="True" LabelsWidth="XXS" ControlSize="M" runat="server" DataField="AllowMatchingCreditMemo" />
                <px:PXLayoutRule ID="PXLayoutRule18" runat="server" LabelsWidth="L" ControlSize="XS" StartGroup="True" GroupCaption="Receipt Matching" />
                <px:PXNumberEdit ID="edReceiptTranDaysBefore" DataField="ReceiptTranDaysBefore" runat="server" />
                <px:PXNumberEdit ID="edReceiptTranDaysAfter" DataField="ReceiptTranDaysAfter" runat="server" />

                <px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartGroup="True" GroupCaption="Weights for Relevance Calculation" />
                <px:PXPanel runat="server" Border="none" RenderStyle="Simple" ID="pnlRelativeWeights" RenderSimple="True">
                    <px:PXLayoutRule ID="PXLayoutRule20" runat="server" LabelsWidth="L" ControlSize="XS" StartColumn="True" />
                    <px:PXNumberEdit ID="edRefNbrCompareWeight" DataField="RefNbrCompareWeight" CommitChanges="True" runat="server" />
					<px:PXLayoutRule runat="server" LabelsWidth="L" ControlSize="xs" ColumnSpan="2"/>
					<px:PXCheckBox ID="chkEmptyRefNbrMatching" runat="server" DataField="EmptyRefNbrMatching" CommitChanges="True"/>
                    <px:PXNumberEdit ID="edDateCompareWeight" DataField="DateCompareWeight" CommitChanges="True" runat="server" />
                    <px:PXNumberEdit ID="edPayeeCompareWeight" DataField="PayeeCompareWeight" CommitChanges="True" runat="server" />
                    <px:PXLayoutRule ID="PXLayoutRule21" runat="server" LabelsWidth="XXS" ControlSize="XS" StartColumn="True" />
                    <px:PXNumberEdit ID="edRefNbrComparePercent" DataField="RefNbrComparePercent" runat="server" Enabled="False" Size="XS" />
                    <px:PXNumberEdit ID="edDateComparePercent" DataField="DateComparePercent" runat="server" Enabled="False" Size="XS" />
                    <px:PXNumberEdit ID="edPayeeComparePercent" DataField="PayeeComparePercent" runat="server" Enabled="False" Size="XS" />
                </px:PXPanel>
                <px:PXLayoutRule ID="PXLayoutRule23" runat="server" LabelsWidth="L" ControlSize="XS" StartGroup="True" GroupCaption="Date Range for Relevance Calculation" />
                <px:PXNumberEdit ID="edDateMeanOffset" DataField="DateMeanOffset" runat="server" />
                <px:PXNumberEdit ID="edDateSigma" DataField="DateSigma" runat="server"/>
                <px:PXLayoutRule ID="PXLayoutRule24" runat="server" LabelsWidth="XXS" ControlSize="M" StartGroup="True" SuppressLabel="true" />
                <px:PXCheckBox ID="chkskipVoided" runat="server" DataField="SkipVoided" />
                <px:PXLayoutRule  runat="server" ID ="PXLayoutRule25" StartGroup="True" ControlSize="XS" LabelsWidth="L" GroupCaption="Expense Receipt Matching" />
                <px:PXNumberEdit  runat="server" ID="edCuryDiffThreshold" DataField="CuryDiffThreshold" />
                <px:PXNumberEdit  runat="server" ID="edAmountWeight" DataField="AmountWeight" CommitChanges="True"/>
                <px:PXTextEdit ID="edRatioInRelevanceCalculationLabel" DataField="RatioInRelevanceCalculationLabel" SkinID="Label" SuppressLabel="True" Enabled="False" runat="server" Width="580px" TextMode="MultiLine" Height="40px"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnResetToDefault" runat="server" CommandName="ResetMatchSettingsToDefault" CommandSourceID="ds" Text="Reset to Default" SyncVisible="false"/>
            <px:PXButton ID="btnSaveClose" runat="server" DialogResult="OK" Text="Save & Close" TabIndex="100" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlCreateRule" runat="server" Key="RuleCreation"
        Caption="Create Rule" AcceptButtonID="btnCreateRuleOk" CaptionVisible="True" DesignView="Content"
        Overflow="Hidden" AutoReload="true" LoadOnDemand="true" HideAfterAction="false">
        <px:PXFormView runat="server" ID="frmCreateRule" DataMember="RuleCreation" SkinID="Transparent" DefaultControlID="edRuleName">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                <px:PXMaskEdit runat="server" DataField="RuleName" ID="edRuleName" CommitChanges="true" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="pxPanelRuleCreationBtns" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnCreateRuleOk" runat="server" DialogResult="OK" Text="Create" />
            <px:PXButton ID="btnCreateRuleCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
