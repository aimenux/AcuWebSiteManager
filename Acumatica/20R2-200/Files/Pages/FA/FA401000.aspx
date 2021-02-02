<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA401000.aspx.cs"
    Inherits="Page_FA401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.AccBalanceByAssetInq" 
        PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edAccountID" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXBranchSelector CommitChanges="True" ID="BranchSelector1" runat="server" DataField="OrgBAccountID" InitialExpandLevel="0" />
            <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
            <px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="PeriodID" AutoRefresh="true"/>
            <px:PXNumberEdit ID="edBalance" runat="server" DataField="Balance" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Assets" 
        FastFilterFields="AssetID" RestrictFields="True" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataKeyNames="AssetID" DataMember="Amts">
                <Columns>
                    <px:PXGridColumn DataField="AssetID" LinkCommand="ViewAsset" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="Status" />
                    <px:PXGridColumn DataField="ClassID" />
                    <px:PXGridColumn DataField="DepreciateFromDate" />
                    <px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn DataField="Department" />
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
