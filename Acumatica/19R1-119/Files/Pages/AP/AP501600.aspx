<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP501600.aspx.cs" Inherits="Page_AP501600" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Items" TypeName="PX.Objects.AP.APExternalTaxCalc" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" DataSourceID="ds" BatchUpdate="True"
        SkinID="PrimaryInquire" SyncPosition="True" FastFilterFields="RefNbr,VendorID,VendorID_BAccountR_acctName,DocDesc" Caption="AP Documents">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector SuppressLabel="True" ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="RefNbr" Width="100px" />
                    <px:PXGridColumn DataField="VendorID" Width="100px" />
                    <px:PXGridColumn DataField="VendorID_BAccountR_acctName" Width="150px" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" Width="100px" />
                    <px:PXGridColumn DataField="DocDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" Width="70px" />
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="CuryID" Width="54px" />
                    <px:PXGridColumn DataField="DocDesc" Width="250px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Layout ShowRowStatus="False" />
    </px:PXGrid>
</asp:Content>
