<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN501000.aspx.cs"
    Inherits="Page_IN501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.Objects.IN.INDocumentRelease" PrimaryView="INDocumentList"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="INDocumentList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXTextEdit ID="edOrigModule" runat="server" DataField="OrigModule" Enabled="False" />
                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" Enabled="False" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" />
                    <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
                    <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty" Enabled="False" />
                    <px:PXNumberEdit ID="edTotalAmount" runat="server" DataField="TotalAmount" Enabled="False" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" AllowCheckAll="True" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrigModule" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocType" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" LinkCommand="ViewDocument"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TotalQty" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TotalCost" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TotalAmount" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDesc" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="ViewDocument"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
