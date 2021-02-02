<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA501600.aspx.cs" Inherits="Page_CA501600" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Items" TypeName="PX.Objects.CA.CAExternalTaxCalc" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True"
        SkinID="PrimaryInquire" Caption="CA Documents" FastFilterFields="AdjRefNbr, TranDesc">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="AdjTranType" Type="DropDownList" />
                    <px:PXGridColumn DataField="AdjRefNbr" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="DrCr" Type="DropDownList" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="TranDesc" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
