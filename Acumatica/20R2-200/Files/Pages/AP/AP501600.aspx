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
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" />
                    <px:PXGridColumn DataField="RefNbr" />
                    <px:PXGridColumn DataField="VendorID" />
                    <px:PXGridColumn DataField="VendorID_BAccountR_acctName" />
                    <px:PXGridColumn DataField="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="DocDesc" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Layout ShowRowStatus="False" />
    </px:PXGrid>
</asp:Content>
