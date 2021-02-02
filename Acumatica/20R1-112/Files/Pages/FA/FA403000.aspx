<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA403000.aspx.cs"
    Inherits="Page_FA403000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.FixedAssetCostEnq"
        PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID" />
            <px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="PeriodID"/>
            <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Accounts/Subs" RestrictFields="True"
        FastFilterFields="AccountID, AcctDescr" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataKeyNames="AccountID,SubID" DataMember="Amts" >
                <Columns>
                    <px:PXGridColumn DataField="BookID" />
                    <px:PXGridColumn DataField="AccountID" />
                    <px:PXGridColumn DataField="AcctDescr" />
                    <px:PXGridColumn DataField="SubID" />
                    <px:PXGridColumn DataField="SubDescr" />
                    <px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn AllowNull="False" DataField="ItdAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="YtdAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="PtdAmt" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="viewDetails" />
    </px:PXGrid>
</asp:Content>
