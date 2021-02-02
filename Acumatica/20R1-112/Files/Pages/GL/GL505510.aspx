<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL505510.aspx.cs" Inherits="Page_GL505510"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="BudgetArticles"
        TypeName="PX.Objects.GL.GLBudgetRelease" BorderStyle="NotSet" SuspendUnloading="False">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="true" BatchUpdate="true"
        SkinID="PrimaryInquire" Caption="Budget Articles" AdjustPageSize="Auto" SyncPosition="True" FastFilterFields="FinYear,AccountID,Description">
        <Levels>
            <px:PXGridLevel DataMember="BudgetArticles">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn DataField="LedgerID" />
                    <px:PXGridColumn DataField="FinYear" LinkCommand="editDetail" />
                    <px:PXGridColumn DataField="AccountID" />
                    <px:PXGridColumn DataField="SubID" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" />
                    <px:PXGridColumn DataField="ReleasedAmount" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Layout ShowRowStatus="False" />
        <Mode AllowUpdate="false" />
    </px:PXGrid>
</asp:Content>
