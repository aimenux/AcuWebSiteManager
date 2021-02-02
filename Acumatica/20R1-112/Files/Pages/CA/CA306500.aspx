<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA306500.aspx.cs" Inherits="Page_BankStatementProtoImpoort" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Header" TypeName="PX.Objects.CA.CABankTransactionsImport">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Unhide" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="Unmatch" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="UnmatchAll" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewDoc" Visible="false" DependOnGrid="grid" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXUploadDialog ID="pnlNewRev" Key="NewRevisionPanel" runat="server" Height="120px" Style="position: static" Width="560px" Caption="Statement File Upload" AutoSaveFile="false" RenderCheckIn="false" SessionKey="ImportStatementProtoFile" />
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Header" TabIndex="5500">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" AllowAddNew="True" AllowEdit="True">
            </px:PXSegmentMask>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr">
            </px:PXSelector>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" CommitChanges="true">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXDateTimeEdit ID="edStartBalanceDate" runat="server" DataField="StartBalanceDate">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edEndBalanceDate" runat="server" DataField="EndBalanceDate">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="edCuryBegBalance" runat="server" DataField="CuryBegBalance" CommitChanges="True">
            </px:PXNumberEdit>
            <px:PXNumberEdit ID="edCuryEndBalance" runat="server" DataField="CuryEndBalance" CommitChanges="True">
            </px:PXNumberEdit>
            <px:PXNumberEdit ID="edCuryDetailsEndBalance" runat="server" DataField="CuryDetailsEndBalance">
            </px:PXNumberEdit>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Height="150px" SkinID="Details" TabIndex="5700" SyncPositionWithGraph="true" SyncPosition="true">
        <ActionBar Position="TopAndBottom" DefaultAction="ViewDetailsDoc" PagerVisible="False" ActionsText="False">
            <CustomItems>
                <px:PXToolBarButton Text="Unhide Transaction" Key="cmdLS" CommandName="Unhide" CommandSourceID="ds" DependOnGrid="grid" StateColumn="Hidden">
                    <ActionBar GroupIndex="1" Order="1" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Unmatch Transaction" Key="cmdLS" CommandName="Unmatch" CommandSourceID="ds" DependOnGrid="grid" StateColumn="DocumentMatched">
                    <ActionBar GroupIndex="1" Order="2" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Unmatch All Transaction" Key="cmdLS" CommandName="UnmatchAll" CommandSourceID="ds" DependOnGrid="grid">
                    <ActionBar GroupIndex="1" Order="3" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View Matched Document" Key="cmdLS" CommandName="ViewDoc" CommandSourceID="ds" DependOnGrid="grid" StateColumn="DocumentMatched">
                    <ActionBar GroupIndex="1" Order="4" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataKeyNames="CashAccountID,TranID" DataMember="Details">
                <RowTemplate>
                    <px:PXCheckBox ID="edProcessed" runat="server" DataField="Processed" Text="Create Doc.">
                    </px:PXCheckBox>
                    <px:PXCheckBox ID="edDocumentMatched" runat="server" DataField="DocumentMatched" Text="Matched">
                    </px:PXCheckBox>
                    <px:PXTextEdit ID="edExtTranID" runat="server" DataField="ExtTranID">
                    </px:PXTextEdit>
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate">
                    </px:PXDateTimeEdit>
                    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc">
                    </px:PXTextEdit>
                    <px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edCuryCreditAmt" runat="server" DataField="CuryCreditAmt">
                    </px:PXNumberEdit>
                    <px:PXTextEdit ID="edInvoiceInfo" runat="server" DataField="InvoiceInfo">
                    </px:PXTextEdit>
                    <px:PXSelector runat="server" DataField="RuleID" AllowEdit="true" ID="edRuleID">
                    </px:PXSelector>
                    <px:PXSelector runat="server" DataField="EntryTypeID1" ID="edEntryTypeID1">
                    </px:PXSelector>
                    <px:PXSelector runat="server" DataField="PaymentMethodID1" ID="edPaymentMethodID1">
                    </px:PXSelector>
                    <px:PXSelector runat="server" DataField="PayeeBAccountID1" ID="edPayeeBAccountID1">
                    </px:PXSelector>
                    <px:PXSelector runat="server" DataField="AcctName" ID="edAcctName">
                    </px:PXSelector>
                    <px:PXDropDown runat="server" DataField="OrigModule1" ID="edOrigModule1">
                    </px:PXDropDown>
                    <px:PXSelector runat="server" DataField="PayeeLocationID1" ID="edPayeeLocationID1">
                    </px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="TranID"/>
                    <px:PXGridColumn DataField="DocumentMatched" Width="60px" TextAlign="Center" Type="CheckBox">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Processed" Width="60px" TextAlign="Center" Type="CheckBox">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Hidden" Width="60px" TextAlign="Center" Type="CheckBox">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ExtTranID" Width="200px" CommitChanges="true">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ExtRefNbr" Width="160px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranDate" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranDesc" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranCode">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryDebitAmt" Width="100px" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CardNumber"/>
                    <px:PXGridColumn DataField="InvoiceInfo" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RuleID" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PayeeName" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="EntryTypeID1" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PaymentMethodID1" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PayeeBAccountID1" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AcctName" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="OrigModule1" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PayeeLocationID1" Width="90px">
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode InitNewRow="True" AllowUpload="True" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
