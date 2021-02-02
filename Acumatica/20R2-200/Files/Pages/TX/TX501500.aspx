<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX501500.aspx.cs" Inherits="Page_TX501500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Items" TypeName="PX.Objects.TX.ExternalTaxPost" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds"
        BatchUpdate="True" SkinID="PrimaryInquire" Caption="Documents" FastFilterFields="RefNbr, DocDesc" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="Module" />
                    <px:PXGridColumn DataField="DocType" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument"/>
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="DocDesc" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="ViewDocument" />
    </px:PXGrid>
</asp:Content>
