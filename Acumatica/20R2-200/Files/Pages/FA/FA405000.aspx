<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA405000.aspx.cs"
    Inherits="Page_FA405000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.FASplitsInq" PrimaryView="Filter" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edAssetID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID" />
            <px:PXSelector CommitChanges="True" ID="edBookID" runat="server" DataField="BookID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" Caption="Split Transactions" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataKeyNames="RefNbr,LineNbr" DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn DataField="AssetID" />
                    <px:PXGridColumn DataField="BookID" />
                    <px:PXGridColumn DataField="RefNbr" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="TranType" RenderEditorText="True" />
                    <px:PXGridColumn DataField="AccountID" />
                    <px:PXGridColumn DataField="SubID" />
                    <px:PXGridColumn DataField="DebitAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CreditAmt" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
