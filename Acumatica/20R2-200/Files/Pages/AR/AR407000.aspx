<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR407000.aspx.cs" Inherits="Page_AR407000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TranFilter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.CABankTransactionsEnqPayments">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDoc" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewStatement" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="TranFilter" TabIndex="100" DefaultControlID="edCashAccountID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="S" LabelsWidth="S" />
            <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDateUI" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="S" LabelsWidth="S" />
            <px:PXDropDown CommitChanges="True" ID="edTranType" runat="server" DataField="TranType" />
            <px:PXSelector CommitChanges="True" ID="edHeaderRefNbr" runat="server" DataField="HeaderRefNbr" AutoRefresh="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" Height="150px" SkinID="Details" TabIndex="1700" FilesIndicator="False">
        <Levels>
            <px:PXGridLevel DataKeyNames="CashAccountID,TranID" DataMember="Result">
                <RowTemplate>
                    <px:PXSelector runat="server" DataField="HeaderRefNbr" AllowEdit="true" ID="edHeaderRefNbr" />
                    <px:PXTextEdit ID="edExtTranID" runat="server" DataField="ExtTranID" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" />
                    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
                    <px:PXTextEdit ID="edStatus" runat="server" DataField="Status" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                    <px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt" />
                    <px:PXNumberEdit ID="edCuryCreditAmt" runat="server" DataField="CuryCreditAmt" />
                    <px:PXTextEdit ID="edInvoiceInfo" runat="server" DataField="InvoiceInfo" />
                    <px:PXSelector runat="server" DataField="RuleID" AllowEdit="true" ID="edRuleID" />
                    <px:PXSelector runat="server" DataField="EntryTypeID" ID="edEntryTypeID" />
                    <px:PXSelector runat="server" DataField="PaymentMethodID" ID="edPaymentMethodID" />
                    <px:PXSelector runat="server" DataField="CATran__ReferenceID" ID="edPayeeBAccountID" />
                    <px:PXSelector runat="server" DataField="CATran__ReferenceName" ID="edAcctName" />
                    <px:PXSelector runat="server" DataField="CATran__OrigModule" ID="edOrigModule" />
                    <px:PXSelector runat="server" DataField="PayeeLocationID" ID="edPayeeLocationID" />

                    <px:PXDropDown ID="edOrigTranType" runat="server" DataField="CATran__OrigTranType" />
                    <px:PXTextEdit ID="edOrigRefNbr" runat="server" DataField="CATran__OrigRefNbr" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="HeaderRefNbr" LinkCommand="ViewStatement" />
                    <px:PXGridColumn DataField="ExtTranID" CommitChanges="true" />
                    <px:PXGridColumn DataField="ExtRefNbr" />
                    <px:PXGridColumn DataField="Status" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="TranDesc" />
                    <px:PXGridColumn DataField="TranCode" />
                    <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="InvoiceInfo" />
                    <px:PXGridColumn DataField="PayeeName" />
                    <px:PXGridColumn DataField="EntryTypeID" />
                    <px:PXGridColumn DataField="PaymentMethodID" />
                    <px:PXGridColumn DataField="RuleID" />
                    <px:PXGridColumn DataField="CATran__OrigModule" />
                    <px:PXGridColumn DataField="PayeeLocationID" />

                    <px:PXGridColumn DataField="CATran__OrigTranType" Type="DropDownList" />
                    <px:PXGridColumn DataField="CATran__OrigRefNbr" LinkCommand="ViewDoc" />
                    <px:PXGridColumn DataField="CATran__ReferenceID" />
                    <px:PXGridColumn DataField="CATran__ReferenceID_BAccountR_acctName" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
