<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM501000.aspx.cs" Inherits="Page_CM501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TranslationParamsFilter" TypeName="PX.Objects.CM.TranslationProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="TranslationParamsFilter" Caption="Translation Processing Parameters" TemplateContainer="" TabIndex="5300">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSelector CommitChanges="True" ID="edTranslDefId" runat="server" DataField="TranslDefId" DisplayMode="Value" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXSelector ID="edLastFinPeriodID" runat="server" DataField="LastFinPeriodID" Enabled="False" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edCuryEffDate" runat="server" DataField="CuryEffDate" />
            <px:PXLabel ID="edEmptyLabel" runat="server" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" Enabled="False" />
            <px:PXSelector ID="edSourceLedgerId" runat="server" DataField="SourceLedgerId" Enabled="False" />
            <px:PXSelector ID="edDestLedgerId" runat="server" DataField="DestLedgerId" Enabled="False" />
            <px:PXTextEdit ID="edSourceCuryID" runat="server" DataField="SourceCuryID" Enabled="False" Size="S" />
            <px:PXTextEdit ID="edDestCuryID" runat="server" DataField="DestCuryID" Enabled="False" Size="S" />
        </Template>
        <AutoSize Container="Window" />
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100;" Width="100%" AllowPaging="True" Caption="Rate Details" ActionsPosition="top" AllowSearch="true"
        SkinID="PrimaryInquire">
        <Levels>
            <px:PXGridLevel DataMember="TranslationCurrencyRateRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edFromCuryID" runat="server" DataField="FromCuryID" />
                    <px:PXSelector ID="PXSelector1" runat="server" DataField="ToCuryID" />
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="CuryRateType_CurrencyRateType_Descr" />
                    <px:PXSelector ID="edCuryRateType" runat="server" DataField="CuryRateType" />
                    <px:PXDateTimeEdit ID="edCuryEffDate" runat="server" DataField="CuryEffDate" />
                    <px:PXNumberEdit ID="edCuryRate" runat="server" DataField="CuryRate" />
                    <px:PXNumberEdit ID="edRateReciprocal" runat="server" DataField="RateReciprocal" />
                    <px:PXNumberEdit ID="edCuryRateID" runat="server" DataField="CuryRateID" Visible="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="CuryRateType" />
                    <px:PXGridColumn DataField="CuryRateType_CurrencyRateType_Descr" />
                    <px:PXGridColumn DataField="FromCuryID" />
                    <px:PXGridColumn DataField="ToCuryID" />
                    <px:PXGridColumn DataField="CuryEffDate" />
                    <px:PXGridColumn DataField="CuryRate" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryMultDiv" Type="DropDownList" />
                    <px:PXGridColumn DataField="RateReciprocal" TextAlign="Right" />
                    <px:PXGridColumn AllowShowHide="False" DataField="CuryRateID" TextAlign="Right" Visible="False" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" />
        <Mode InitNewRow="False" />
    </px:PXGrid>
</asp:Content>
