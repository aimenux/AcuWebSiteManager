<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA302010.aspx.cs" Inherits="Page_CA302010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.CAReconEnq">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXSmartPanel ID="panel" Key="cashAccountFilter" runat="server" Caption="Create Reconciliation" DesignView="Content" CaptionVisible="True">
        <px:PXFormView ID="addReconForm" runat="server" DataSourceID="ds" Width="100%" DataMember="cashAccountFilter" Caption="Create Reconciliation" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
                <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
                <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
                    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Add Reconciliation" />
                    <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
                </px:PXPanel>
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="PXSegmentMask1" runat="server" DataField="AccountID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;" Width="100%" Caption="Reconciliation Statements" SkinID="PrimaryInquire"
        FastFilterFields="CashAccountID, CashAccountID_CashAccount_descr, ReconNbr" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="CAReconRecords">
                <Columns>
                    <px:PXGridColumn DataField="Status" Width="60px" />
                    <px:PXGridColumn DataField="CashAccountID" Width="81px" />
                    <px:PXGridColumn DataField="CashAccountID_CashAccount_descr" TextCase="Upper" Width="150px" />
                    <px:PXGridColumn DataField="ReconNbr" Width="90px" LinkCommand="ViewDoc" />
                    <px:PXGridColumn DataField="ReconDate" Width="120px"/>
                    <px:PXGridColumn DataField="LastReconDate" Width="120px"/>
                    <px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="CuryReconciledDebits" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn DataField="CuryReconciledCredits" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn DataField="CuryBalance" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="CountDebit" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="CountCredit" TextAlign="Right" Width="110px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode InitNewRow="True" />
    </px:PXGrid>
</asp:Content>
