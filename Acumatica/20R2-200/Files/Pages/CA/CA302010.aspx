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
                    <px:PXGridColumn DataField="Status" />
                    <px:PXGridColumn DataField="CashAccountID" />
                    <px:PXGridColumn DataField="CashAccountID_CashAccount_descr" TextCase="Upper" />
                    <px:PXGridColumn DataField="ReconNbr" LinkCommand="ViewDoc" />
                    <px:PXGridColumn DataField="ReconDate"/>
                    <px:PXGridColumn DataField="LastReconDate"/>
                    <px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryReconciledDebits" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryReconciledCredits" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryBalance" TextAlign="Right" />
                    <px:PXGridColumn DataField="CountDebit" TextAlign="Right" />
                    <px:PXGridColumn DataField="CountCredit" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode InitNewRow="True" />
    </px:PXGrid>
</asp:Content>
