<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CM504000.aspx.cs" Inherits="Page_CM504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CM.RevalueAPAccounts" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Revaluation Summary" TemplateContainer="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edCuryEffDate" runat="server" DataField="CuryEffDate" />
            <px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXNumberEdit ID="edTotalRevalued" runat="server" AllowNull="False" DataField="TotalRevalued" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100;" Width="100%" ActionsPosition="Top" Caption="Revaluation Details" 
        SkinID="PrimaryInquire" FastFilterFields="AccountID, AccountID_Account_Description, VendorID, VendorID_Vendor_AcctName">
        <Levels>
            <px:PXGridLevel DataMember="APAccountList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowMove="False" AllowSort="False" />
                    <px:PXGridColumn DataField="BranchID"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID" />
                    <px:PXGridColumn DataField="AccountID_Account_Description" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID" />
                    <px:PXGridColumn DataField="VendorID_Vendor_AcctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryRateTypeID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryRate" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryFinYtdBalance" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinYtdBalance" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPrevRevalued" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinYtdRevalued" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPtdRevalued" TextAlign="Right" />
                    <px:PXGridColumn DataField="LastRevaluedFinPeriodID" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
