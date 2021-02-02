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
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="Module" Width="100px" />
                    <px:PXGridColumn DataField="DocType" Width="100px" />
                    <px:PXGridColumn DataField="RefNbr" Width="100px" LinkCommand="ViewDocument"/>
                    <px:PXGridColumn DataField="DocDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="70px" />
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="CuryID" Width="80px" />
                    <px:PXGridColumn DataField="DocDesc" Width="250px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <ActionBar DefaultAction="ViewDocument" />
    </px:PXGrid>
</asp:Content>
