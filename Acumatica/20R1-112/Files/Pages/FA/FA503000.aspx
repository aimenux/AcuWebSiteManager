<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA503000.aspx.cs"
    Inherits="Page_FA503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetTranRelease" Visible="True" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Options" DataMember="Filter" DefaultControlID="edOrigin">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="SM" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="Origin" ID="edOrigin" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="PrimaryInquire" Caption="Documents" FastFilterFields="RefNbr, DocDesc" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="FADocumentList">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Origin" RenderEditorText="True" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDesc" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
