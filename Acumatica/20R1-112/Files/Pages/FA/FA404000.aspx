<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA404000.aspx.cs"
    Inherits="Page_FA404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.FACostDetailsInq" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edAssetID" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID" DataMember="_FixedAsset_" />
            <px:PXSelector CommitChanges="True" ID="edStartPeriodID" runat="server" DataField="StartPeriodID"/>
            <px:PXSelector CommitChanges="True" ID="edEndPeriodID" runat="server" DataField="EndPeriodID"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Transactions" 
        FastFilterFields="RefNbr, TranDesc" RestrictFields="True" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataKeyNames="RefNbr,LineNbr" DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn DataField="BookID" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
                    <px:PXGridColumn DataField="BatchNbr" LinkCommand="ViewBatch" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="BranchID" />
                    <px:PXGridColumn DataField="TranType" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TranDesc" />
                    <px:PXGridColumn AllowNull="False" DataField="DebitAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="CreditAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="AccountID" />
                    <px:PXGridColumn DataField="SubID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
