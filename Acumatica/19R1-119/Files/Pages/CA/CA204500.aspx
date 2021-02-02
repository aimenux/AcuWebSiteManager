<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA204500.aspx.cs" Inherits="Page_CA204500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="Rule" TypeName="PX.Objects.CA.CABankTranRuleMaint">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataMember="Rule" DataSourceID="ds"
        Width="100%" >
        <AutoSize Container="Window" Enabled="True" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" ControlSize="sm" />
            <px:PXSelector runat="server" DataField="RuleCD" ID="edRuleCD" AutoAdjustColumns="true" DisplayMode="Value" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit runat="server" DataField="Description" ID="edDescription" />
            <px:PXLayoutRule runat="server" GroupCaption="Matching Criteria"></px:PXLayoutRule>

            <px:PXDropDown runat="server" DataField="BankDrCr" ID="edBankDrCr" CommitChanges="true" />
            <px:PXSegmentMask runat="server" DataField="BankTranCashAccountID" ID="edCashAccountID" CommitChanges="true" />
            <px:PXSelector runat="server" DataField="TranCuryID" ID="edCuryID" />
            <px:PXMaskEdit runat="server" DataField="TranCode" ID="edTranCode" />
            <px:PXTextEdit runat="server" DataField="BankTranDescription" ID="edBankTranDescription" />
            <px:PXCheckBox runat="server" DataField="MatchDescriptionCase" ID="edMatchDescriptionCase" />
            <px:PXCheckBox runat="server" DataField="UseDescriptionWildcards" ID="edUseDescriptionWildcards" />
            <px:PXDropDown runat="server" DataField="AmountMatchingMode" ID="edAmountMatchingMode" CommitChanges="true" />
            <px:PXNumberEdit runat="server" DataField="CuryTranAmt" ID="edCuryTranAmt" AllowNull="true" />
            <px:PXNumberEdit runat="server" DataField="MaxCuryTranAmt" ID="edMaxCuryTranAmt" AllowNull="true" />

            <px:PXLayoutRule runat="server" StartColumn="true"></px:PXLayoutRule>
            <px:PXCheckBox runat="server" DataField="IsActive" ID="edIsActive" />

            <px:PXLayoutRule runat="server" GroupCaption="Output" LabelsWidth="m" ControlSize="m"></px:PXLayoutRule>
            <px:PXDropDown runat="server" DataField="Action" ID="edAction" CommitChanges="true" />
            <px:PXSelector runat="server" DataField="DocumentEntryTypeID" ID="edDocumentEntryType" AutoRefresh="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
