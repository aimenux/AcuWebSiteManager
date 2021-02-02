<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA402000.aspx.cs" Inherits="Page_CA402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TranFilter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.CABankTransactionsEnq">
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
                    <px:PXSelector runat="server" DataField="RuleID" AllowEdit="true" ID="edRuleID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="HeaderRefNbr" LinkCommand="ViewStatement" Width="90px" />
                    <px:PXGridColumn DataField="ExtTranID" CommitChanges="true" Width="90px" />
                    <px:PXGridColumn DataField="ExtRefNbr" Width="90px" />
                    <px:PXGridColumn DataField="Status" Width="90px" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="TranDesc" Width="180px" />
                    <px:PXGridColumn DataField="TranCode" Width="90px" />
                    <px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="InvoiceInfo" Width="90px" />
                    <px:PXGridColumn DataField="PayeeName" Width="90px" />
                    <px:PXGridColumn DataField="EntryTypeID" Width="120px" />
                    <px:PXGridColumn DataField="PaymentMethodID" Width="90px" />
                    <px:PXGridColumn DataField="RuleID" Width="90px" />
                    <px:PXGridColumn DataField="CATran__OrigModule" Width="70px" />
                    <px:PXGridColumn DataField="PayeeLocationID" Width="90px" />

                    <px:PXGridColumn DataField="CATran__OrigTranType" Type="DropDownList" Width="90px" />
                    <px:PXGridColumn DataField="CATran__OrigRefNbr" LinkCommand="ViewDoc" Width="90px" />
                    <px:PXGridColumn DataField="CATran__ReferenceID" Width="120px" />
                    <px:PXGridColumn DataField="CATran__ReferenceID_BAccountR_acctName" Width="150px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
