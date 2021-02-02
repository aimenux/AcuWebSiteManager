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
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="AdjTranType" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="AdjRefNbr" Width="100px" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="DrCr" Type="DropDownList" Width="120px" />
                    <px:PXGridColumn DataField="TranDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="70px" />
                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="CuryID" Width="80px" />
                    <px:PXGridColumn DataField="TranDesc" Width="250px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
