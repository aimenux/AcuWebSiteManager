<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL509000.aspx.cs" Inherits="Page_GL509000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ConsolSetupRecords"
        TypeName="PX.Objects.GL.GLConsolReadMaint">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True"
        AllowSearch="true" SkinID="PrimaryInquire" AdjustPageSize="Auto" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="ConsolSetupRecords">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
                        AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="BranchID" Width="100px" />
                    <px:PXGridColumn DataField="LedgerId" Width="100px" />
                    <px:PXGridColumn DataField="Description" Width="250px" />
                    <px:PXGridColumn DataField="Url" Width="250px" />
                    <px:PXGridColumn DataField="SourceLedgerCD" Width="100px" />
                    <px:PXGridColumn DataField="SourceBranchCD" Width="100px" />
                    <px:PXGridColumn DataField="LastPostPeriod" Width="100px" />
                    <px:PXGridColumn DataField="LastConsDate" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode AllowUpdate="false" />
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
