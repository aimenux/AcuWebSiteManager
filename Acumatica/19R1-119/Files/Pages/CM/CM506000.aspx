<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM506000.aspx.cs" Inherits="Page_CM506000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CM.RevalueGLAccounts" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Revaluation Summary" TemplateContainer="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edCuryEffDate" runat="server" DataField="CuryEffDate" />
            <px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXNumberEdit ID="edTotalRevalued" runat="server" DataField="TotalRevalued" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100;" Width="100%" ActionsPosition="Top" Caption="Revaluation Details" 
        SkinID="PrimaryInquire" FastFilterFields="AccountID, AccountID_Account_Description">
        <Levels>
            <px:PXGridLevel DataMember="GLAccountList">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" AllowCheckAll="True" AllowMove="False" AllowSort="False" />
                    <px:PXGridColumn DataField="BranchID" Width="81px" />
                    <px:PXGridColumn DataField="AccountID" Width="60px" />
                    <px:PXGridColumn DataField="AccountID_Account_Description" Width="180px" />
                    <px:PXGridColumn DataField="SubID" Width="90px" />
                    <px:PXGridColumn DataField="CuryRateTypeID" Width="60px" />
                    <px:PXGridColumn DataField="CuryRate" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="CuryFinYtdBalance" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FinYtdBalance" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FinYtdRevalued" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FinPtdRevalued" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="LastRevaluedFinPeriodID" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
