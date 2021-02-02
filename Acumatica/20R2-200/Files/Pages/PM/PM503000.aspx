<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM503000.aspx.cs"
    Inherits="Page_PM503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="true" PrimaryView="Filter" TypeName="PX.Objects.PM.BillingProcess"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edInvoiceDate" runat="server" DataField="InvoiceDate" />
            <px:PXSelector CommitChanges="True" ID="edInvFinPeriodID" runat="server" DataField="InvFinPeriodID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Unbilled Invoices"
        SkinID="PrimaryInquire" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True" FastFilterFields="ProjectCD,Description,CustomerID,CustomerName">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                    <px:PXTextEdit ID="edCustomerName" runat="server" DataField="CustomerID_Customer_acctName" Enabled="False" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
                    <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" Enabled="False" />
                    <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" Enabled="False" />
                    <px:PXDateTimeEdit ID="edNextDate" runat="server" DataField="NextDate" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="ProjectCD" Label="Project" LinkCommand="viewDocumentProject"/>
                    <px:PXGridColumn DataField="Description" Label="Description" />
                    <px:PXGridColumn DataField="CustomerID" Label="Customer ID" />
                    <px:PXGridColumn DataField="CustomerID_Customer_acctName" Label="Customer Name" />
                    <px:PXGridColumn DataField="FromDate" Label="From" />
                    <px:PXGridColumn DataField="NextDate" Label="To" />
                    <px:PXGridColumn DataField="LastDate" Label="Last Billing Date" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False" />
    </px:PXGrid>
</asp:Content>
